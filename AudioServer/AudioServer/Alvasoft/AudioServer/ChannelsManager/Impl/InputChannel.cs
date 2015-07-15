using System;
using Alvasoft.AudioServer.Configuration;
using Alvasoft.AudioServer.ChannelsManager.Impl.Devices;

namespace Alvasoft.AudioServer.ChannelsManager.Impl
{
    /// <summary>
    /// Входной канал.
    /// </summary>
    public class InputChannel
    {
        private InputChannelInfo channelInfo; // ссылка на конфигурацию
        private IInputDevice device; // Ссылка на физическое устройство  

        /// <summary>
        /// Канал на звуковом устройстве.
        /// </summary>
        /// <param name="aChannelInfo">Данные канала, отношения.</param>
        /// <param name="aDevice">Физическое устройство.</param>
        /// <param name="aGroup">Группа, к которой принадлежит канал.</param>        
        public InputChannel(InputChannelInfo aChannelInfo, IInputDevice aDevice)
        {            
            if (aChannelInfo == null) {
                throw new ArgumentNullException("aChannelInfo");
            }

            if (aDevice == null) {
                throw new ArgumentNullException("aDevice");
            }

            device = aDevice;
            channelInfo = aChannelInfo;            
        }

        /// <summary>
        /// Возвращает конфигурацию входного канала.
        /// </summary>
        /// <returns>Конфигурация.</returns>
        public InputChannelInfo GetChannelInfo()
        {
            return channelInfo;
        }

        /// <summary>
        /// Начинает запись голосового собщения на текущем канале.
        /// </summary>
        /// <param name="aChannelsIds">Идентификаторы выходных каналов.</param>
        /// <param name="aConnectionHashCode">Инициатор записи.</param>
        public void StartRecord(int[] aChannelsIds, int aConnectionHashCode)
        {                        
            device.StartRecord(channelInfo.GetChannelNumber(), aChannelsIds, aConnectionHashCode);
        }

        /// <summary>
        /// Прекращает запись сообщения на канале.
        /// </summary>
        /// <param name="aConnectionHashCode">Инифиатор записи.</param>
        public void StopRecord(int aConnectionHashCode)
        {
            device.StopRecord(channelInfo.GetChannelNumber(), aConnectionHashCode);            
        }
    }
}
