using System;
using System.Threading;
using Alvasoft.AudioServer.Configuration;
using log4net;

namespace Alvasoft.AudioServer.ChannelsManager.Impl
{
    /// <summary>
    /// Класс - канала на физическом устройстве.
    /// </summary>
    public class OutChannel
    {
        private static readonly ILog Logger = LogManager.GetLogger("OutChannel");

        private OutChannelInfo channelInfo; // ссылка на конфигурацию
        private OutputDevice device; // Ссылка на физическое устройство        
        private ChannelGroup group; // ссылка на группу
        private bool isWorking;// Метка работы потока, просмотра очереди сообщений

        private readonly SoundsPriorityQueue soundsQueue = new SoundsPriorityQueue(); // очередь сообщений
        private Thread workingThread;

        /// <summary>
        /// Канал на звуковом устройстве.
        /// </summary>
        /// <param name="aChannelInfo">Данные канала, отношения.</param>
        /// <param name="aDevice">Физическое устройство.</param>
        /// <param name="aGroup">Группа, к которой принадлежит канал.</param>        
        public OutChannel(OutChannelInfo aChannelInfo, OutputDevice aDevice, ChannelGroup aGroup)
        {            
            if (aChannelInfo == null) {
                throw new ArgumentNullException("aChannelInfo");
            }

            if (aDevice == null) {
                throw new ArgumentNullException("aDevice");
            }

            if (aGroup == null) {
                throw new ArgumentNullException("aGroup");
            }            

            device = aDevice;
            channelInfo = aChannelInfo;
            group = aGroup;
            
            isWorking = false;
        }

        /// <summary>
        /// Состояние работы
        /// </summary>
        /// <returns>True - если канал в потоке работает, False - иначе.</returns>
        public bool IsWorking()
        {
            return isWorking;
        }

        /// <summary>
        /// Получить описание канала.
        /// </summary>
        /// <returns>Описание канала.</returns>
        public OutChannelInfo GetChannelInfo()
        {
            return channelInfo;
        }

        /// <summary>
        /// Получить связанное устройство.
        /// </summary>
        /// <returns>Связанное устройство.</returns>
        public OutputDevice GetOutputDevice()
        {
            return device;
        }

        /// <summary>
        /// Положить новое звуковое сообщение в потокобезопасную очередь с приоритетами.
        /// </summary>
        /// <param name="aMessage">Сообщение.</param>
        public void InsertMessage(SoundMessage aMessage)
        {
            soundsQueue.InsertMessage(aMessage);
        }

        /// <summary>
        /// Начать новый поток работы.
        /// </summary>
        public void BeginWork()
        {
            if (isWorking) {
                throw new Exception("Channel is already running");
            }

            isWorking = true;
            workingThread = new Thread(WorkFunction);
            workingThread.Start();
        }

        /// <summary>
        /// Прекратить работу потока.
        /// </summary>
        public void StopWork()
        {
            if (isWorking) {
                isWorking = false;                
                workingThread.Join(); // дождаться завершения
            }            
        }

        /// <summary>
        /// Функция крутится в потоке, вытаскивает сообщения из очереди и отправляет на device.
        /// </summary>
        private void WorkFunction()
        {
            while (isWorking) {
                var message = soundsQueue.GetTopMessage();
                if (message == null) {
                    Thread.Sleep(100);
                }
                else {                    
                    try {
                        var channelNumber = channelInfo.GetChannelNumber();

                        Logger.Info("Channel: " + channelInfo.GetId() + " блокируем группу.");
                        group.LockGroup(); // потокобезопасно ждем освобождения
                        Logger.Info("Channel: " + channelInfo.GetId() + " группа заблокирована.");                                                
                        device.WriteDataToChannel(message.GetSound(), channelNumber);                        
                    }
                    finally {
                        group.UnLockGroup();
                    }

                }
            }
        }

        /// <summary>
        /// Возвращает текущий уровесь звука на канале.
        /// </summary>
        /// <returns>Уровень звука.</returns>
        public int GetCurrentLevel()
        {
            return device.GetCurrentLevel(channelInfo.GetChannelNumber());
        }
    }
}
