using Alvasoft.Utils.Common;

namespace Alvasoft.AudioServer.Configuration
{
    /// <summary>
    /// Интерфейс конфигурации входного канала.
    /// </summary>
    public interface InChannelInfo : IdentifiableNameable
    {
        /// <summary>
        /// Возвращает конфигурацию устройства ввода длля текущего канала.
        /// </summary>
        /// <returns></returns>
        InputDeviceInfo GetDevice();

        /// <summary>
        /// Возвращает номер канала.
        /// <para>Физический номер канала на устройстве. </para>
        /// </summary>
        /// <returns>Номер канала.</returns>
        int GetChannelNumber();
    }
}
