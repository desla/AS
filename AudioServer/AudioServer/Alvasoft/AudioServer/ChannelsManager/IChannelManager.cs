using Alvasoft.AudioServer.ChannelsManager.Impl;

namespace Alvasoft.AudioServer.ChannelsManager
{
    /// <summary>
    /// Интерфейс менеджера каналов.
    /// </summary>
    public interface IChannelManager
    {
        /// <summary>
        /// Отправляет сообщение в очередь указанного канала.
        /// </summary>
        /// <param name="aMessage">Сообщение.</param>
        /// <param name="aChannelId">Логический идентификатор канала.</param>
        void ProcessMessage(SoundMessage aMessage, long aChannelId);

        /// <summary>
        /// Начинает управление.
        /// </summary>
        void StartManage();

        /// <summary>
        /// Прекращает управление.
        /// </summary>
        void StopManage();

        /// <summary>
        /// Возвращает канал по идентификатору.
        /// </summary>
        /// <param name="aChannelId">Идентифкатор канала.</param>
        /// <returns>Канал или null.</returns>
        OutputChannel GetChannelById(int aChannelId);

        /// <summary>
        /// Начинает запись звука на канале.
        /// </summary>
        /// <param name="aInputLineId">Входной канал.</param>
        /// <param name="aChannelsIds">Выходные каналы.</param>
        /// <param name="aConnectionHashCode">Инициатор записи.</param>
        void StartRecord(int aInputLineId, int[] aChannelsIds, int aConnectionHashCode);

        /// <summary>
        /// Прекращает запись звука на канале.
        /// </summary>
        /// <param name="aConnectionHashCode">Инициатор записи.</param>
        void StopRecord(int aConnectionHashCode);
    }
}
