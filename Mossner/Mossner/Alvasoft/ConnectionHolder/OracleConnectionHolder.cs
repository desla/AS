using System;
using System.Data;
using Alvasoft.BridgeConfiguration;
using log4net;
//using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;

namespace Alvasoft.ConnectionHolder
{
    /// <summary>
    /// Реализует механизм поддержания связи с Oracle-сервером.
    /// </summary>
    public class OracleConnectionHolder : ConnectionHolderBase
    {
        private static readonly ILog Logger = LogManager.GetLogger("OracleConnectionHolder");

        private readonly OracleConnection connection = new OracleConnection();
        private string serverHost;        
        private string userName;
        private string password;        

        /// <summary>
        /// Создает OracleConnectionHolder.
        /// </summary>
        /// <param name="aServerHost">Имя/Адрес сервера.</param>
        /// <param name="aUserName">Имя пользователя.</param>
        /// <param name="aPassword">Пароль пользователя.</param>
        public OracleConnectionHolder(string aServerHost, string aUserName, string aPassword)
        {
            if (string.IsNullOrEmpty("aServerHost")) {
                throw new ArgumentNullException("aServerHost");
            }

            if (string.IsNullOrEmpty("aUserName")) {
                throw new ArgumentNullException("aUserName");
            }

            if (string.IsNullOrEmpty("aPassword")) {
                throw new ArgumentNullException("aPassword");
            }

            serverHost = aServerHost;
            userName = aUserName;
            password = aPassword;

            connection.ConnectionString = CreateConnectionString();
        }

        /// <summary>
        /// Конструктор на основе конфигурации.
        /// </summary>
        /// <param name="aCnfg">Конфигурация.</param>
        public OracleConnectionHolder(ConnectionConfiguration aCnfg) 
            : this(aCnfg.Host, aCnfg.User, aCnfg.Password)
        {
            SetReconnectionInterval(aCnfg.ReconnectionInterval);
            SetCheckConnectionInterval(aCnfg.ReconnectionInterval);
            SetLastOperationAllowedTime((int)(aCnfg.ReconnectionInterval / 1000));
        }

        /// <summary>
        /// Вернуть OracleConnection.
        /// </summary>
        /// <returns>OracleConnection.</returns>
        public OracleConnection GetOracleConnection()
        {
            return connection;
        }        

        /// <summary>
        /// Закрыть подключение.
        /// </summary>
        public override void CloseConnection()
        {
            checkConnectionTimer.Stop();
            connection.Close();

            if (callback != null) {
                callback.OnDisconnected(this);
            }            
        }

        /// <summary>
        /// Подключен ли сервер.
        /// </summary>
        /// <returns>True - если соединение с сервером открыто, false - иначе.</returns>
        public override bool IsConnected()
        {
            return connection.State == ConnectionState.Open;
        }

        /// <summary>
        /// Обработать ошибку.
        /// </summary>
        /// <param name="aError">Ошибка.</param>
        /// <returns>True - если удалось обработать ошибку, false - иначе.</returns>
        public override bool ProcessError(Exception aError)
        {            
            if (aError is OracleException) {
                if (callback != null && !IsConnectionProcessActive) {
                    callback.OnError(this, aError);
                }

                if ((!IsConnected() || !TestConnectionIsOpen()) && !IsConnectionProcessActive) {
                    CloseConnection(); // должны закрыть соединение, так как connection.State не обязательно Closed
                    TryReconnect();
                }

                return true;
            }

            Logger.Error(GetHolderName() + " - ошибка не обработана - " + aError);

            return false;
        }

        /// <summary>
        /// Тестировать соединение.
        /// </summary>
        /// <returns>True - если соединение открыто, false - иначе.</returns>
        protected override bool TestConnectionIsOpen()
        {
            try {                
                using (var command = new OracleCommand("select * from v$version", connection)) {
                    using (command.ExecuteReader()) {
                        Logger.Debug(GetHolderName() + " проверка соединения: успешно");
                        return true;
                    }
                }
            }
            catch {                
                return false;
            }
        }

        /// <summary>
        /// Выполнить опасное подключение. Может завершиться с исключением.
        /// </summary>
        protected override void DangerousConnect()
        {
            connection.Open();
        }

        /// <summary>
        /// Сформировать строку подключение для OracleConnection.
        /// </summary>
        /// <returns>Старока подключения.</returns>
        private string CreateConnectionString()
        {
            string connectionString;
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password)) {
                connectionString = string.Format("Data Source={0};User Id={1};Password={2};", 
                                                    serverHost, 
                                                    userName,
                                                    password);
            }
            else {
                throw new NotImplementedException();
            }

            return connectionString;
        }
    }
}
