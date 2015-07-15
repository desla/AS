using System;
using System.Runtime.InteropServices;
using OPCAutomation;

namespace StableConnection.ConnectionHolder
{
    /// <summary>
    /// Описывает состояния OpcServer'а.
    /// </summary>
    enum OPCServerState
    {
        CONNECTED = 1,
        DISCONNECTED = 6
    }

    /// <summary>
    /// Реализует механизм поддержания связи с сервером OPC.
    /// </summary>
    public class OpcConnectionHolder : ConnectionHolderBase
    {
        private readonly OPCServer server = new OPCServer();
        private string serverName;
        private string serverHost;
        private bool isConnected;

        /// <summary>
        /// Создает OpcConnectionHolder.
        /// </summary>
        /// <param name="aServerName">Имя OPC-сервера.</param>
        /// <param name="aServerHost">Адрес OPC-сервера. Если null - сервер находится на локальной машине.</param>
        public OpcConnectionHolder(string aServerName, string aServerHost = null)
        {
            if (string.IsNullOrEmpty(aServerName)) {
                throw new ArgumentNullException("aServerName");
            }

            serverName = aServerName;
            serverHost = aServerHost;
        }

        /// <summary>
        /// Возвращает OpcServer.
        /// </summary>
        /// <returns>OpcServer.</returns>
        public OPCServer GetOpcServer()
        {
            return server;
        }

        /// <summary>
        /// Возвращает имя текущей реализации ConnectionHolder.
        /// </summary>
        /// <returns>Имя.</returns>
        public override string GetHolderName()
        {
            return "OPC Server";
        }

        /// <summary>
        /// Закрывает соединение.
        /// </summary>
        public override void CloseConnection()
        {            
            server.Disconnect();
            isConnected = false;
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
            return isConnected;
        }

        /// <summary>
        /// Обработать ошибку.
        /// </summary>
        /// <param name="aError">Ошибка.</param>
        /// <returns>True - если обработать ошибку удалось, false - иначе.</returns>
        public override bool ProcessError(Exception aError)
        {
            if (aError is COMException) {                
                if (callback != null && !IsConnectionProcessActive) {
                    callback.OnError(this, aError);
                }                

                if (!IsConnectionProcessActive) {
                    CloseConnection();
                    TryReconnect();                    
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Тестировать соединение с сервером.
        /// </summary>
        /// <returns>True - если соединение с сервером открыто, false - иначе.</returns>
        protected override bool TestConnectionIsOpen()
        {
            try {
                return isConnected = server.ServerState == (int) OPCServerState.CONNECTED;
            }
            catch {
                return isConnected = false;
            }            
        }

        /// <summary>
        /// Выполнить опасное подключение. Может вернуть исключение в случае неудачи.
        /// </summary>
        protected override void DangerousConnect()
        {
            server.Connect(serverName, serverHost);
            isConnected = true;
        }
    }
}
