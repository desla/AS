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

            sessionCleaner = new Timer(5 * 60 * 1000);
            sessionCleaner.Elapsed += CleanSession;            
        }

        /// <summary>
        /// Удаляет отключившиеся сессии.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CleanSession(object sender, ElapsedEventArgs e)
        {
            sessionCleaner.Stop();
            try {                                
                var currentSessions = new List<AbstractClientSession>();
                lock (sessions) {
                    currentSessions.AddRange(sessions);
                }
                Logger.Info("Удаление неактивных подключений...");
                Logger.Info("Всего подключений: " + currentSessions.Count);
                var failedSessionsCount = 0;
                var oldSessionsCount = 0;
                var currentTime = DateTime.Now;
                for (var i = 0; i < currentSessions.Count; ++i) {
                    if (currentSessions[i].GetClientConnection().IsConnected() == false) {
                        failedSessionsCount++;
                        OnCloseConnection(currentSessions[i]);
                    } 
                    else if (currentSessions[i].CreatedTime.AddMinutes(3) <= currentTime) {
                        oldSessionsCount++;
                        OnCloseConnection(currentSessions[i]);
                    }
                }

                Logger.Info(string.Format("Закрыто подключений: " +
                                          "{0} неактивных; " +
                                          "{1} по таймауту.", 
                                          failedSessionsCount, oldSessionsCount));
            }
            catch (Exception ex) {
                Logger.Error("Ошибка во время очистки пула соединений: " + ex.Message);
            }
            finally {
                sessionCleaner.Start();
            }
        }

        /// <inheritdoc />
        public void OnCreatedConnection(AsyncTcpServer aAsyncTcpServer, ClientConnection aClientConnection)
        {
            // Создаем новый экземпляр сессии.
            var session = CreateSession(aClientConnection);
            lock (sessions) {
                sessions.Add(session);
            }            
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
            Logger.Info("Закрываем соединение...");
            lock (sessions) {
                sessions.Remove(aClientSession);
            }            

            aClientSession.FreeConnection();

            if (callback != null) {
                callback.OnCloseConnection(this, aClientSession);
            }

            Logger.Info("Соединение закрыто.");
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
            lock (sessions) {
                return sessions.Count;
            }            
        }

        /// <summary>
        /// Создает экземпляр сессии.
        /// </summary>
        /// <param name="aClientConnection">Клиентское подключение.</param>
        /// <returns>Сессия.</returns>
        protected abstract AbstractClientSession CreateSession(ClientConnection aClientConnection);
    }
}