using System;
using System.Threading;
using Alvasoft.AudioServer.Configuration;

namespace Alvasoft.AudioServer.ChannelsManager.Impl
{
    /// <summary>
    /// Группа, объединяющая каналы
    /// </summary>
    public class ChannelGroup
    {        
        private readonly Mutex groupLock = new Mutex(); // для блокирования группы
        private bool isBusy;
        private ChannelGroupInfo groupInfo;

        /// <summary>
        /// Конструктор группы каналов.
        /// </summary>
        /// <param name="aGroupInfo">Ссылка на конфигурацию.</param>
        public ChannelGroup(ChannelGroupInfo aGroupInfo)
        {
            if (aGroupInfo == null) {
                throw new ArgumentNullException("aGroupInfo");
            }

            groupInfo = aGroupInfo;
        }

        /// <summary>
        /// Метка занятости группы (если какой-либо канал говорит).
        /// </summary>
        /// <returns>True - если какой-либо канал в группе занят, False - если все каналы свободны.</returns>
        public bool IsBusy()
        {
            return isBusy;
        }

        /// <summary>
        /// Возвращает конфигурацию группы о группы.
        /// </summary>
        /// <returns>Ссылка на конфигурацию.</returns>
        public ChannelGroupInfo GetChannelGroupInfo()
        {
            return groupInfo;
        }

        /// <summary>
        /// Дождаться своей очереди в группе.
        /// </summary>
        public void LockGroup()
        {
            groupLock.WaitOne();           
            isBusy = true;                            
        }

        /// <summary>
        /// Освободить группу
        /// </summary>
        public void UnLockGroup()
        {             
            isBusy = false;                            
            groupLock.ReleaseMutex();
        }
    }
}
