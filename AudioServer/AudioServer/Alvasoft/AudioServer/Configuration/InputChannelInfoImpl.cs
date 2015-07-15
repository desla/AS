using Alvasoft.Utils.Common;

namespace Alvasoft.AudioServer.Configuration
{
    /// <summary>
    /// Реализация конфигурации входного канала.
    /// </summary>
    public class InputChannelInfoImpl : IdentifiableNameableImpl, InputChannelInfo
    {
        private InputDeviceInfo device;
        private int channelNumber;       
        private long deviceId;       

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public InputChannelInfoImpl()
        {
            device = null;
            channelNumber = 0;            
            deviceId = 0;            
        }

        /// <inheritdoc />
        public InputDeviceInfo GetDevice()
        {
            return device;
        }

        /// <summary>
        /// Изменяет устройство вывода.
        /// </summary>
        /// <param name="aDevice">Устройство вывода.</param>
        public void SetDevice(InputDeviceInfo aDevice)
        {
            device = aDevice;
        }

        /// <inheritdoc />
        public int GetChannelNumber() {
            return channelNumber;
        }

        /// <summary>
        /// Изменяет номер канала.
        /// </summary>
        /// <param name="aChannelNumber">Номер канала.</param>
        public void SetChannelNumber( int aChannelNumber ) {
            channelNumber = aChannelNumber;
        }

        /// <summary>
        /// Возвращает идентификатор устройства вывода.
        /// </summary>
        /// <returns>Номер устройства вывода.</returns>
        public long GetDeviceId() 
        {
            return deviceId;
        }

        /// <summary>
        /// Изменяет идентификатор устройства.
        /// </summary>
        /// <param name="aDeviceId">Идентификатор устройства.</param>
        public void SetDeviceId( long aDeviceId ) 
        {
            deviceId = aDeviceId;
        }
    }
}
