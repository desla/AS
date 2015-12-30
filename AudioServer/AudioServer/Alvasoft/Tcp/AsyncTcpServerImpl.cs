using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Alvasoft.Utils.Activity;
using log4net;

namespace Alvasoft.Tcp
{
    /// <summary>
    /// Реализация асинхронного TCP сервера.
    /// </summary>
    public class AsyncTcpServerImpl : InitializableImpl, AsyncTcpServer
    {
        private static readonly ILog Logger = LogManager.GetLogger("AsyncTcpServerImpl");

        private AsyncTcpServerCallback callback;
        private IPEndPoint listenEndPoint = new IPEndPoint(IPAddress.Any, 0);

        private Thread listenThread;        
        private ManualResetEvent listenSignal = new ManualResetEvent(false);
        private Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        /// <summary>
        /// Инициализирует TCP-сервер.
        /// </summary>
        protected override void DoInitialize()
        {
            Logger.Info("Инициализация AsyncTcpServerImpl...");

            try {
                listenThread = new Thread(() => {
                    while (true) {
                        listenSignal.Reset();
                        listener.BeginAccept(AcceptCallback, null);
                        listenSignal.WaitOne();                        
                    }
                });

                listener.Bind(listenEndPoint);
                listener.Listen(50);                  
                listenThread.Start();
            }
            catch (Exception ex) {
                Logger.Error(ex.Message);
            }

            Logger.Info("Инициализация AsyncTcpServerImpl завершена.");
        }

        /// <summary>
        /// Асинхронно принимает входящее подключение.
        /// </summary>
        /// <param name="ar">Параметры.</param>
        private void AcceptCallback(IAsyncResult ar)
        {
            try {
                var clientSocket = listener.EndAccept(ar);
                listenSignal.Set();
                var clientConnection = new ClientConnectionImpl(clientSocket);
                if (callback != null) {
                    callback.OnCreatedConnection(this, clientConnection);
                }
            }
            catch (SocketException) {
                Logger.Error("Не удалось подключиться.");
            }
            catch (ObjectDisposedException) {
            }
        }

        /// <summary>
        /// Деинициализирует TCP-сервер при отключении.
        /// </summary>
        protected override void DoUninitialize()
        {
            Logger.Info("Деинициализация AsyncTcpServerImpl...");

            try {                
                listener.Dispose();                
                listenThread.Abort();
            }
            catch (Exception ex) {
                Logger.Info(ex.Message);
            }

            Logger.Info("Деинициализация AsyncTcpServerImpl завершена.");
        }

        /// <summary>
        /// Возвращает точку подключения.
        /// </summary>
        /// <returns>IPEndPoint.</returns>
        public IPEndPoint GetListenEndPoint()
        {
            return listenEndPoint;
        }

        /// <summary>
        /// Устанавливает callback для обратной связи.
        /// </summary>
        /// <param name="aCallback">Callback.</param>
        public void SetCallback(AsyncTcpServerCallback aCallback)
        {
            callback = aCallback;
        }
    }
}