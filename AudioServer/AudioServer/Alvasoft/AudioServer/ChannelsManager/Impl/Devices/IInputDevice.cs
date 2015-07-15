using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alvasoft.AudioServer.ChannelsManager;

namespace AudioServer.Alvasoft.AudioServer.ChannelsManager.Impl.Devices
{
    /// <summary>
    /// Интерфейс входного устройства.
    /// </summary>
    public interface IInputDevice : IDisposable
    {
        /// <summary>
        /// Возвращает количество каналов на устройстве.
        /// </summary>
        /// <returns></returns>
        int GetChannelsCount();

        /// <summary>
        /// Начинает запись звука на канале.
        /// </summary>
        /// <param name="aInChannelNumber">Номер канала.</param>
        /// <param name="aOutChannelsIds">Идентификаторы выходных каналов.</param>
        /// <param name="aRecordOwner">Инициатор записи.</param>
        void StartRecord(int aInChannelNumber, int[] aOutChannelsIds, int aRecordOwner);

        /// <summary>
        /// Заканчивает запись звука на канале.
        /// </summary>
        /// <param name="aChannelNumber">Идентификатор канала.</param>
        /// <param name="aRecordOwner">Инициатор записи.</param>
        void StopRecord(int aChannelNumber, int aRecordOwner);

        void SetCallback(IChannelManager aCallback);
    }
}
