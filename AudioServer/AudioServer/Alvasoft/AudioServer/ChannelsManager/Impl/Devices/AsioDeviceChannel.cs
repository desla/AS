using NAudio.Wave;

namespace Alvasoft.AudioServer.ChannelsManager.Impl.Devices
{
    /// <summary>
    /// Структура для объединения логических элементов в главном классе AsioOutputDevice.
    /// </summary>
    public class AsioDeviceChannel
    {
        /// <summary>
        /// Буфер для сохранения звука.
        /// </summary>
        public BufferedWaveProvider Buffer;
        /// <summary>
        /// Текущий уровень звука на канале.
        /// </summary>
        public int CurrentLevel;
        /// <summary>
        /// Объект для блокировке при параллельном доступе.
        /// </summary>
        public object Locker;
        /// <summary>
        /// Инициатор записи звука для входных каналов.
        /// </summary>
        public int RecordOwner;
        /// <summary>
        /// Метка работы канала для входных каналов.
        /// </summary>
        public bool IsBusy;
    }
}
