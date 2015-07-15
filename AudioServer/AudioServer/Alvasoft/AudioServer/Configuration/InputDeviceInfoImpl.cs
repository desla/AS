using System;
using System.Collections.Generic;
using System.Linq;
using Alvasoft.Utils.Common;

namespace Alvasoft.AudioServer.Configuration
{
    /// <summary>
    /// Реализация конфигурации устройства ввода.
    /// </summary>
    public class InputDeviceInfoImpl : IdentifiableNameableImpl, InputDeviceInfo
    {
        private List<InputChannelInfoImpl> channels = new List<InputChannelInfoImpl>();
        private DeviceType deviceType = DeviceType.DEFAULT;

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public InputDeviceInfoImpl()
        {
        }

        /// <inheritdoc />
        public int GetChannelsCount() {
            return channels.Count();
        }

        /// <inheritdoc />
        public InputChannelInfo GetChannel( int aIndex ) {
            return channels.ElementAt( aIndex );
        }

        public DeviceType GetDeviceType()
        {
            return deviceType;
        }

        public void SetDeviceType(string aStrDeviceType)
        {
            var values = Enum.GetValues(typeof(DeviceType));
            foreach (var value in values) {
                var t = (DeviceType)value;
                var source = t.ToString().ToLower();
                var argument = aStrDeviceType.ToLower();
                if (source.Equals(argument)) {
                    deviceType = t;
                    return;
                }
            }

            throw new ArgumentException("Неизвестный тип устройства: " + aStrDeviceType);
        }

        /// <returns>Каналы.</returns>
        public List<InputChannelInfoImpl> GetChannels() {
            return channels;
        }       
    }
}
