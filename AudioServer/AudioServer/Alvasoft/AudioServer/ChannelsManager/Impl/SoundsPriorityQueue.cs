using System;
using System.Collections.Generic;

namespace Alvasoft.AudioServer.ChannelsManager.Impl
{
    /// <summary>
    /// Очередь с приоритетами. Минимальный приоритет 0, максимальный настраивается (по-умолчанию 255).
    /// </summary>
    public class SoundsPriorityQueue
    {
        // для потокобезопасности
        private object queueLock = new object();
        private List<SoundMessage> messageList = new List<SoundMessage>();
        private readonly uint maxPriority;

        /// <summary>
        /// Сохраняет приоритет
        /// </summary>
        /// <param name="aMaxPriority">Максимально возможный приоритет сообщения в очереди. По-умолчанию = 255.</param>
        public SoundsPriorityQueue(uint aMaxPriority = 255) 
        {
            maxPriority = aMaxPriority;
        }

        /// <summary>
        /// Добавить сообщение в очередь.
        /// </summary>
        /// <param name="aMessage">Сообщение.</param>
        public void InsertMessage(SoundMessage aMessage)
        {
            if (aMessage.GetPriority() > maxPriority) {
                throw new ArgumentOutOfRangeException(string.Format("Priority must be less than {0}.", maxPriority));
            }

            lock (queueLock) {
                messageList.Add(aMessage);
                messageList.Sort(Comparer);
            }
        }

        /// <summary>
        /// Получить из очереди сообщение с максимальным приоритетом.
        /// </summary>
        /// <returns>Сообщение с максимальным приоритетом или null, если очередь пуста.</returns>
        public SoundMessage GetTopMessage()
        {
            lock (queueLock) {
                if (messageList.Count > 0) {
                    var topItem = messageList[0];
                    messageList.RemoveAt(0);
                    return topItem;
                }

                return null;
            }
        }

        /// <summary>
        /// Результат проверки очереди на наличие элементов.
        /// </summary>
        /// <returns>True, если очередь не пустая, False иначе.</returns>
        public bool IsEmpty()
        {
            return messageList.Count == 0;
        }

        /// <summary>
        /// Сортируем так, чтобы сообщения с высоким приоритетом были вначале очереди.
        /// </summary>
        /// <param name="aFirst">X.</param>
        /// <param name="aSecond">Y.</param>
        /// <returns>Результат сравнения.</returns>
        private static int Comparer(SoundMessage aFirst, SoundMessage aSecond)
        {
            return aFirst.GetPriority() <= aSecond.GetPriority() ? 0 : -1;
        }
    }
}
