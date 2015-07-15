using System;
using System.Collections.Generic;
using System.Linq;
using Alvasoft.Utils.Common;

namespace Alvasoft.AudioServer.Configuration
{
    /// <summary>
    ///     Реализация описания устройства вывода.
    /// </summary>
    public class OutputDeviceInfoImpl : IdentifiableNameableImpl, OutputDeviceInfo
    {
        private List<OutputChannelInfoImpl> channels = new List<OutputChannelInfoImpl>();
        private DeviceType deviceType = DeviceType.DEFAULT;

        /// <inheritdoc />
        public int GetChannelsCount()
        {
            return channels.Count();
        }

        /// <inheritdoc />
        public OutputChannelInfo GetChannel(int aIndex)
        {
            return channels.ElementAt(aIndex);
        }

        /// <returns>Каналы.</returns>
        public List<OutputChannelInfoImpl> GetChannels()
        {
            return channels;
        }

        /// <summary>
        /// Возвращает тип устройства.
        /// </summary>
        /// <returns>Тип устройства.</returns>
        public DeviceType GetDeviceType()
        {
            return deviceType;
        }

        /// <summary>
        /// Устанавливает тип устройтсва. По-умолчания - default.
        /// </summary>
        /// <param name="aStrDeviceType">Тип устройства в строковом формате.</param>
        /// <exception cref="NotImplementedException">Возникает, если тип не найден.</exception>
        public void SetDeviceType(string aStrDeviceType)
        {
            var values = Enum.GetValues(typeof (DeviceType));
            foreach (var value in values) {
                var t = (DeviceType) value;
                var source = t.ToString().ToLower();
                var argument = aStrDeviceType.ToLower();
                if (source.Equals(argument)) {
                    deviceType = t;
                    return;
                }
            }

            throw new ArgumentException("Неизвестный тип устройства: " + aStrDeviceType);
        }
    }
}