using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alvasoft.Utils.Common;

namespace Alvasoft.AudioServer.Configuration
{
    /// <summary>
    /// Интерфейс конфигурации устройства ввода.
    /// </summary>
    public interface InputDeviceInfo : IdentifiableNameable
    {
        /// <summary>
        /// Возвращает число каналов на устройстве.
        /// </summary>
        /// <returns></returns>
        int GetChannelsCount();

        /// <summary>
        /// Возвращает конфигурацию канала по индексу.
        /// </summary>
        /// <param name="aIndex">Индекс.</param>
        /// <returns>Конфигурация канала на устройстве.</returns>
        InputChannelInfo GetChannel(int aIndex);

        /// <summary>
        /// Возвращает тип устройства.
        /// </summary>
        /// <returns>Тип устройства.</returns>
        DeviceType GetDeviceType();

        /// <summary>
        ///     Устанавливает тип устройтсва. По-умолчания - default.
        /// </summary>
        /// <param name="aStrDeviceType">Тип устройства в строковом формате.</param>
        /// <exception cref="NotImplementedException">Возникает, если тип не найден.</exception>
        void SetDeviceType(string aStrDeviceType);
    }
}
