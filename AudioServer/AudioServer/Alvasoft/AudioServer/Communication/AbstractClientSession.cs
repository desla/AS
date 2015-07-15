using Alvasoft.Tcp;

namespace Alvasoft.AudioServer.Communication
{
    /// <summary>
    /// Абстрактная клиентская сессия.
    /// </summary>
    public abstract class AbstractClientSession
    {        
        private ClientConnection clientConnection;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aClientConnection">Клиентское подключение.</param>
        public AbstractClientSession(ClientConnection aClientConnection)
        {
            clientConnection = aClientConnection;
        }

        /// <summary>
        /// Клиентское подключение.
        /// </summary>
        /// <returns></returns>
        public ClientConnection GetClientConnection()
        {
            return clientConnection;
        }

        /// <summary>
        /// Освобождает ресурсы подключения.
        /// </summary>
        public void FreeConnection()
        {
            clientConnection.FreeConnection();
        }
    }
}