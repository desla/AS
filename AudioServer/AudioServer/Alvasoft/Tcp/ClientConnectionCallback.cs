namespace Alvasoft.Tcp
{
    /// <summary>
    /// Интерфейс обратных вызовов клиентского подключения.
    /// </summary>
    public interface ClientConnectionCallback
    {
        /// <summary>
        /// Получены данные.
        /// </summary>
        /// <param name="aClientConnection">Клиентское подключение.</param>
        /// <param name="aBuffer">Буфер с данными.</param>
        /// <param name="aBufferSize">Размер буфера.</param>
        void OnReceivedFrom(ClientConnection aClientConnection, byte[] aBuffer, int aBufferSize);
    }
}