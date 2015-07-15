using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using StableConnection.ConnectionHolder.ConnectionCallback;

namespace StableConnection.ConnectionHolder
{
    /// <summary>
    /// Базовый класс, где реализуются алгоритмы поддержания подключения к удаленному серверу.
    /// </summary>
    public abstract class ConnectionHolderBase
    {
        private const double DEFAULT_RECONNECTION_INTERVAL = 1000; // 1 секунда
        private Timer reconnectionTimer; // Вызывает функцию переподключения
        protected bool IsConnectionProcessActive { get; private set;} // Указывает идет или нет процесс подключения   
        private object connectionProcessLock = new object();

        private const double DEFAULT_CHECK_CONNECTION_INTERVAL = 1000 * 5; // milliseconds
        private long lastOperationAllowedTimeInSeconds = 30; // seconds
        private DateTime lastOperationTime = DateTime.MinValue; // метка времени последней операции на сервере
        private Timer checkConnectionTimer; // Таймер вызывает метод проверки подключения
        private bool isCheckConnectionProcessActive = false;
        private object checkConnectionProcessingLock = new object();

        protected IConnectionHolderCallback callback;

        /// <summary>
        /// В конструкторе создаются и настраиваются таймеры.
        /// </summary>
        protected ConnectionHolderBase()
        {
            reconnectionTimer = new Timer(DEFAULT_RECONNECTION_INTERVAL);
            reconnectionTimer.AutoReset = true;
            reconnectionTimer.Elapsed += TryReconnect;

            checkConnectionTimer = new Timer(DEFAULT_CHECK_CONNECTION_INTERVAL);
            checkConnectionTimer.Elapsed += CheckLastOperationTimeAndReconnect;
            checkConnectionTimer.AutoReset = true;
            checkConnectionTimer.Start();
        }

        /// <summary>
        /// Устанавливает метку времени последней операции на текущее время.
        /// </summary>
        public void UpdateLastOperationTime()
        {
            lastOperationTime = DateTime.Now;
        }

        /// <summary>
        /// Установить Callback.
        /// </summary>
        /// <param name="aCallback">Callback.</param>
        public void SetCallback(IConnectionHolderCallback aCallback)
        {
            callback = aCallback;
        }

        /// <summary>
        /// Установить интервал, через который будет происходить вызов функции подключения.
        /// </summary>
        /// <param name="aReconnectionInterval">Время в миллисекундах.</param>
        public void SetReconnectionInterval(double aReconnectionInterval)
        {
            if (aReconnectionInterval < 1) {
                throw new ArgumentOutOfRangeException("aReconnectionInterval");
            }

            reconnectionTimer.Interval = aReconnectionInterval;
        }

        /// <summary>
        /// Установить интервал, через который будет происходить вызов функции проверки подключения.
        /// </summary>
        /// <param name="aCheckConnectionInterval">Время в миллисекундах.</param>
        public void SetCheckConnectionInterval(double aCheckConnectionInterval)
        {
            if (aCheckConnectionInterval < 1) {
                throw new ArgumentOutOfRangeException("aCheckConnectionInterval");
            }

            checkConnectionTimer.Interval = aCheckConnectionInterval;
        }

        /// <summary>
        /// Установить максимально позволенный интервал между операциями на сервере, 
        /// по истечению которого будет происходить проверка подключения.
        /// </summary>
        /// <param name="aSeconds">Время в секундах.</param>
        public void SetLastOperationAllowedTime(int aSeconds)
        {
            if (aSeconds < 1) {
                throw new ArgumentOutOfRangeException("aSeconds");
            }

            lastOperationAllowedTimeInSeconds = aSeconds;
        }

        /// <summary>
        /// Подключиться.
        /// </summary>
        public void OpenConnection()
        {
            TryReconnect();
        }

        /// <summary>
        /// Получить имя конкрутной реализации.
        /// </summary>
        /// <returns></returns>
        public abstract string GetHolderName();        

        /// <summary>
        /// Выполнить отключение от сервера.
        /// </summary>
        public abstract void CloseConnection();

        /// <summary>
        /// С высокой долей вероятности возвращает реальное состояние подключения.
        /// Используйте функцию TestConnectionIsOpen для подтверждения подключения.
        /// </summary>
        /// <returns>True - если подключение окрыто, false - иначе.</returns>
        public abstract bool IsConnected();

        /// <summary>
        /// Обработать ошибку.
        /// </summary>
        /// <param name="aError">Ошибка.</param>
        /// <returns>True - если обработать ошибку удалось, false - иначе.</returns>
        public abstract bool ProcessError(Exception aError);

        /// <summary>
        /// Возвращает реальное состояние подключения.
        /// </summary>
        /// <returns>True - если подключение открыто, false - иначе.</returns>
        protected abstract bool TestConnectionIsOpen();
        
        /// <summary>
        /// Выполнить опасное подключение. В случае неудачи может вызвать исключение.
        /// </summary>
        protected abstract void DangerousConnect();

        /// <summary>
        /// Выполнить безопасное подключение. Вызывается таймером.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        protected void TryReconnect(object sender = null, ElapsedEventArgs e = null)
        {            
            // Контролируем, чтобы функция не работала в нескольких потоках.
            lock (connectionProcessLock) {
                if (IsConnectionProcessActive) {                    
                    return;
                }

                IsConnectionProcessActive = true;
            }

            try {
                Console.WriteLine(GetHolderName() + ": Попытка установления связи с сервером...");

                DangerousConnect(); // может выкинуть исключение, а может и не выкинуть.

                if (TestConnectionIsOpen()) {
                    if (reconnectionTimer.Enabled) {
                        reconnectionTimer.Stop();
                    }
                    
                    UpdateLastOperationTime();
                    Console.WriteLine(GetHolderName() + ": Соединение с сервером установлено.");
                    if (callback != null) {
                        callback.OnConnected(this);
                    }
                }
                else {
                    throw new Exception();
                }
            }
            catch {
                Console.WriteLine(GetHolderName() + ": Соединение не установлено.");
                if (!reconnectionTimer.Enabled) {
                    reconnectionTimer.Start();
                }
            }
            finally {
                IsConnectionProcessActive = false;
            }
        }

        /// <summary>
        /// Проверка метки времени последней операции на сервере. Вызывается таймером.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void CheckLastOperationTimeAndReconnect(object sender = null, ElapsedEventArgs e = null)
        {            
            // Контролируем, чтобы функция не работала в нескольких экземплярах в разных потоках.
            lock (checkConnectionProcessingLock) {
                if (isCheckConnectionProcessActive) {
                    return;
                }

                isCheckConnectionProcessActive = true;
            }

            try {
                if (DateTime.Now < lastOperationTime.AddSeconds(lastOperationAllowedTimeInSeconds)) {
                    return;
                }

                // Если уже идет процесс подключения.
                if (reconnectionTimer.Enabled || IsConnectionProcessActive) {
                    return;
                }

                if (!IsConnected() || !TestConnectionIsOpen()) {
                    Console.WriteLine("Соединение с сервером отсутствует.");
                    if (callback != null) {
                        callback.OnDisconnected(this);
                    }

                    reconnectionTimer.Start();
                }
                else {
                    UpdateLastOperationTime();
                }
            }
            finally {
                isCheckConnectionProcessActive = false;
            }
        }        
    }
}
