using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using Alvasoft.Tcp;
using Alvasoft.Utils.Activity;
using log4net;

namespace Alvasoft.AudioServer.Communication
{
    /// <summary>
    /// Абстрактный слушатель сервера.
    /// </summary>
    public abstract class AbstractServerListener : InitializableImpl, AsyncTcpServerCallback
    {
        private static readonly ILog Logger = LogManager.GetLogger("AbstractServerListener");

        private AsyncTcpServer asyncTcpServer = new AsyncTcpServerImpl();
        private ServerListenerCallback callback;
        private List<AbstractClientSession> sessions = new List<AbstractClientSession>();

        private Timer sessionCleaner;

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public AbstractServerListener()
        {
            asyncTcpServer.SetCallback(this);
            callback = null;

            sessionCleaner = new Timer(5000);
            sessionCleaner.Elapsed += CleanSession;            
        }

        /// <summary>
        /// Удаляет отключившиеся сессии.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CleanSession(object sender, ElapsedEventArgs e)
        {
            try {
                var closedSessions = new List<AbstractClientSession>();
                foreach (var session in sessions) {
                    if (!session.GetClientConnection().IsConnected()) {
                        closedSessions.Add(session);
                    }
                }

                foreach (var session in closedSessions) {
                    OnCloseConnection(session);
                }
            }
            catch {
            }
        }

        /// <inheritdoc />
        public void OnCreatedConnection(AsyncTcpServer aAsyncTcpServer, ClientConnection aClientConnection)
        {
            // Создаем новый экземпляр сессии.
            var session = CreateSession(aClientConnection);
            sessions.Add(session);
            if (callback != null) {
                callback.OnCreatedSession(this, session);
            }
            Logger.Info("Создана новая сессия.");
        }

        /// <summary>
        /// Реагирует при закрытии соединения с клиентом.
        /// </summary>
        /// <param name="aClientSession"></param>
        public void OnCloseConnection(AbstractClientSession aClientSession)
        {
            sessions.Remove(aClientSession);
            aClientSession.FreeConnection();

            if (callback != null) {
                callback.OnCloseConnection(this, aClientSession);
            }
        }

        /// <summary>
        /// Возвращает прослушиваемый интерфейс и номер порта.
        /// </summary>
        /// <returns>Прослушиваемый интерфейс и номер порта.</returns>
        public IPEndPoint GetListenEndPoint()
        {
            return asyncTcpServer.GetListenEndPoint();
        }

        /// <summary>
        /// Инициализирует интерфейс обратных вызовов.
        /// </summary>
        /// <param name="aCallback">Интерфейс обратных вызовов.</param>
        public void SetCallback(ServerListenerCallback aCallback)
        {
            callback = aCallback;
        }

        /// <inheritdoc />
        protected override void DoInitialize()
        {
            Logger.Info("Инициализация AbstractServerListener...");
            asyncTcpServer.Initialize();
            sessionCleaner.Start();
            Logger.Info("Инициализация AbstractServerListener завершена.");
        }

        /// <inheritdoc />
        protected override void DoUninitialize()
        {
            Logger.Info("Деинициализация AbstractServerListener...");
            sessionCleaner.Stop();            
            asyncTcpServer.Uninitialize();
            Logger.Info("Деинициализация AbstractServerListener завершена.");
        }

        /// <summary>
        /// Возвращает количество сессий.
        /// </summary>
        /// <returns>Количество сессий.</returns>
        public int GetSessionsCount()
        {
            return sessions.Count;
        }

        /// <summary>
        /// Создает экземпляр сессии.
        /// </summary>
        /// <param name="aClientConnection">Клиентское подключение.</param>
        /// <returns>Сессия.</returns>
        protected abstract AbstractClientSession CreateSession(ClientConnection aClientConnection);
    }
}