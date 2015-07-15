namespace Alvasoft.Tcp
{
    /// <summary>
    /// Интерфейс клиентского подключения.
    /// </summary>
    public interface ClientConnection
    {
        /// <summary>
        /// Инициализирует интерфейс обратных вызовов.
        /// </summary>
        /// <param name="aCallback">Интерфейс обратных вызовов.</param>
        void SetCallback(ClientConnectionCallback aCallback);

        /// <summary>
        /// Посылает данные.
        /// </summary>
        /// <param name="aBuffer">Буфер с данными.</param>
        /// <param name="aBufferSize">Размер буфера.</param>        
        bool SendAsync(byte[] aBuffer, int aBufferSize);

        /// <summary>
        /// Возвращает состояние подключения.
        /// </summary>
        /// <returns></returns>
        bool IsConnected();

        /// <summary>
        /// Освобождает все ресурсы подключения.
        /// </summary>
        void FreeConnection();
    }
}