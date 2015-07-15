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
        private List<InChannelInfoImpl> channels = new List<InChannelInfoImpl>();

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
        public InChannelInfo GetChannel( int aIndex ) {
            return channels.ElementAt( aIndex );
        }

        /// <returns>Каналы.</returns>
        public List<InChannelInfoImpl> GetChannels() {
            return channels;
        }
    }
}
