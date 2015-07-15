namespace Alvasoft.AudioServer.Configuration
{
    /// <summary>
    /// Тип устройства.
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// Устройство, работу с которым производить стандартными средствами Windows.
        /// </summary>
        DEFAULT = 1,

        /// <summary>
        /// Устройство с поддержкой протокола Asio.
        /// </summary>
        ASIO = 2    
    };
}
