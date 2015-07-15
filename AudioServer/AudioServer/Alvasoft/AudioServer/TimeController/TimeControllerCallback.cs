namespace Alvasoft.AudioServer.TimesController
{
    /// <summary>
    /// Интерфейс обратных вызовов для контроллера времени.
    /// </summary>
    public interface TimeControllerCallback
    {
        /// <summary>
        /// Команды на объявление времени.
        /// </summary>
        /// <param name="aChannelIds">Идентивикаторы выходных каналов для объявления времени.</param>
        /// <param name="aPriority">Приоритет команды.</param>
        /// <param name="aPrefixSound">Префикс для объявления времени.</param>
        /// <param name="aTimePhrase">Фраза для объявления.</param>
        void OnTimeAnnounce(int[] aChannelIds, int aPriority, byte[] aPrefixSound, string aTimePhrase);
    }
}
