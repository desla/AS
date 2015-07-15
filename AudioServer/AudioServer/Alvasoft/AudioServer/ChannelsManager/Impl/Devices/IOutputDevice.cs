using System;

namespace Alvasoft.AudioServer.ChannelsManager.Impl.Devices
{
    /// <summary>
    /// Интерфейс, описывающий выходное устройство.
    /// </summary>
    public interface IOutputDevice : IDisposable
    {
        /// <summary>
        /// Возвращает текущий уровень звука на канале.
        /// </summary>
        /// <param name="aChannelId">Номер канала.</param>
        /// <returns>Уровень звука.</returns>
        int GetCurrentLevel(int aChannelId);

        /// <summary>
        /// Устанавливает количество сэмплов в секунду - параметры воспроизведения.
        /// По-умолчанию 22050.
        /// </summary>
        /// <param name="aSamplePerSecond">Количество сэмплов в секунду.</param>
        void SetSamplePerSecond(int aSamplePerSecond);

        /// <summary>
        /// Устанавливает количество байт в сэмпле - параметры воспроизведения.
        /// По-умолчанию 2.
        /// </summary>
        /// <param name="aBytePerSample">Количество байт в сэмпле.</param>
        void SetBytePerSample(int aBytePerSample);

        /// <summary>
        /// Возвращает количество каналов на устройстве.
        /// </summary>
        /// <returns>Количество каналов на устройстве.</returns>
        int GetChannelsCount();

        /// <summary>
        /// Записываем звук в буфер устройства.
        /// </summary>
        /// <param name="aSoundData">Записываемые данные.</param>
        /// <param name="aChannelId">Номер канала воспроизведения. Если -1 => воспроизводим на всех каналах.</param>
        void WriteDataToChannel(byte[] aSoundData, int aChannelId);
    }
}
