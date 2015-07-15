using System;
using System.Data;
using System.Data.SqlClient;
using Alvasoft.WagstaffBridge.BridgeConfiguration;
using log4net;

namespace Alvasoft.WagstaffBridge.ConnectionHolder
{
    /// <summary>
    /// Реализует механизм поддержания связи с сервером MSSQL.
    /// </summary>
    public class MsSqlConnectionHolder : ConnectionHolderBase
    {
        private static readonly ILog Logger = LogManager.GetLogger("MsSqlConnectionHolder");        

        private readonly SqlConnection connection = new SqlConnection();        
        private string serverHost;
        private string database;
        private string userId;
        private string password;        

        /// <summary>
        /// Создает MsSqlConnectionHolder.
        /// </summary>
        /// <param name="aServerHost">Имя/Адрес удаленного сервера.</param>
        /// <param name="aDatabase">Имя базы данных.</param>
        /// <param name="aUserId">Имя пользователя.</param>
        /// <param name="aPassword">Пароль пользователя.</param>
        public MsSqlConnectionHolder(string aServerHost, string aDatabase, string aUserId = null, string aPassword = null)
        {
            if (string.IsNullOrEmpty(aServerHost)) {
                throw new ArgumentNullException("aServerHost");
            }

            if (string.IsNullOrEmpty(aDatabase)) {
                throw new ArgumentNullException("aDatabase");
            }

            serverHost = aServerHost;
            database = aDatabase;
            userId = aUserId;
            password = aPassword;

            connection.ConnectionString = CreateConnectionString();
        }

        /// <summary>
        /// Конструктор на основе конфигурации.
        /// </summary>
        /// <param name="aCnfg">Конфигурация.</param>
        public MsSqlConnectionHolder(ConnectionConfiguration aCnfg)
            : this(aCnfg.Host, aCnfg.Database, aCnfg.User, aCnfg.Password)
        {
            SetReconnectionInterval(aCnfg.ReconnectionInterval);
            SetCheckConnectionInterval(aCnfg.ReconnectionInterval);
            SetLastOperationAllowedTime((int)(aCnfg.ReconnectionInterval / 1000));
        }

        /// <summary>
        /// Возвращает SqlConnection.
        /// </summary>
        /// <returns>SqlConnection.</returns>
        public SqlConnection GetSqlConnection()
        {        
            return connection;
        }        

        /// <summary>
        /// Закрывает текущее соединение.
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
        /// Подключено ли текущее соединение.
        /// </summary>
        /// <returns>True - если подключение открыто, false - иначе.</returns>
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
            if (aError is SqlException) {
                if (callback != null && !IsConnectionProcessActive) {
                    callback.OnError(this, aError);
                }

                if ((!IsConnected() || !TestConnectionIsOpen()) && !IsConnectionProcessActive) {                    
                    TryReconnect();
                }

                return true;
            }

            Logger.Error(GetHolderName() + " - ошибка не обработана - " + aError);
            return false;
        }

        /// <summary>
        /// Тестировать текущее соединение.
        /// </summary>
        /// <returns>True - если подключение открыто, false - иначе.</returns>
        protected override bool TestConnectionIsOpen()
        {
            try {
                LockConnection();
                using (var command = new SqlCommand("select @@VERSION", connection)) {
                    using (command.ExecuteReader()) {
                        return true;
                    }
                }
            }
            catch {
                return false;
            }
            finally {
                ReleaseConnection();
            }
        }

        /// <summary>
        /// Выполнить подключение. Может выдать исключение в случае неудачи.
        /// </summary>
        protected override void DangerousConnect()
        {
            connection.Open();
        }

        /// <summary>
        /// Сформировать строку подключение для SqlConnection.
        /// </summary>
        /// <returns>Строка подключения.</returns>
        private string CreateConnectionString()
        {
            string connectionString;
            if (userId != null && password != null) {
                connectionString = string.Format(@"Server={0};Database={1};User id={2};Password={3}",
                                     serverHost, database, userId, password);
            }
            else {                
                throw new NotImplementedException();
            }

            return connectionString;
        }
    }
}
