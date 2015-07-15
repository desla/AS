namespace Alvasoft.Tcp
{
    /// <summary>
    /// Интерфейс обратных вызовов асинхронного TCP-сервера.
    /// </summary>
    public interface AsyncTcpServerCallback
    {
        /// <summary>
        /// Создано клиентское подключение.
        /// </summary>
        /// <param name="aAsyncTcpServer">Асинхронный TCP-сервер.</param>
        /// <param name="aClientConnection">Клиентское подключение.</param>
        void OnCreatedConnection(AsyncTcpServer aAsyncTcpServer, ClientConnection aClientConnection);
    }
}