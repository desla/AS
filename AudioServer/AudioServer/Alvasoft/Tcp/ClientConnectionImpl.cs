using System;
using System.Net.Sockets;
using System.Text;
using log4net;

namespace Alvasoft.Tcp
{
    /// <summary>
    /// Реализация клиентского подключения к серверу.
    /// </summary>
    public class ClientConnectionImpl : ClientConnection
    {
        private static readonly ILog Logger = LogManager.GetLogger("AsyncTcpServerImpl");

        private ClientConnectionCallback callback;
        private Socket socket;        
        private byte[] buffer = new byte[1024];

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aAcceptedSocket">Подключившийся сокет.</param>
        public ClientConnectionImpl(Socket aAcceptedSocket)
        {
            socket = aAcceptedSocket;
            socket.BeginReceive(buffer, 0, buffer.Length, 0, ReadCallback, null);
        }

        /// <summary>
        /// Асинхронно получает данные данные от клиента.
        /// </summary>
        /// <param name="ar">Параметры.</param>
        private void ReadCallback(IAsyncResult ar)
        {
            try {
                var bytesRead = socket.EndReceive(ar);
                if (bytesRead > 0) {
                    if (callback != null) {
                        callback.OnReceivedFrom(this, buffer, bytesRead);
                    }
                }

                socket.BeginReceive(buffer, 0, buffer.Length, 0, ReadCallback, null);
            }
            catch (SocketException) {
                //Logger. ("Нет соединения");
            }
            catch (ObjectDisposedException) {
            }            
        }

        /// <summary>
        /// Устанавливает callback для обратной связи.
        /// </summary>
        /// <param name="aCallback">Callback.</param>
        public void SetCallback(ClientConnectionCallback aCallback)
        {
            callback = aCallback;
        }

        /// <summary>
        /// Асинхронно выполняет передачу данных клиенту.
        /// </summary>
        /// <param name="aBuffer">Буфер данных.</param>
        /// <param name="aBufferSize">Размер буфера данных.</param>
        /// <returns>Результат. True - передача выполнена успешно, false - иначе.</returns>
        public bool SendAsync(byte[] aBuffer, int aBufferSize)
        {
            if (socket.Connected) {
                socket.BeginSend(aBuffer, 0, aBuffer.Length, 0, SendCallback, null);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Асинхронно завершает передачу данных клиенту.
        /// </summary>
        /// <param name="ar">Параметры.</param>
        private void SendCallback(IAsyncResult ar)
        {
            try {
                socket.EndSend(ar);
            }
            catch {
            }
        }

        /// <summary>
        /// Возвращает состояние подключения.
        /// </summary>
        /// <returns>True - соединение есть, false - иначе.</returns>
        public bool IsConnected()
        {
            return socket.Connected;
        }

        /// <summary>
        /// Освобождает ресурсы подключения.
        /// </summary>
        public void FreeConnection()
        {            
            socket.Dispose();                   
        }
    }
}