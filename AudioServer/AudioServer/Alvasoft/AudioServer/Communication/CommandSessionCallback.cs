namespace Alvasoft.AudioServer.Communication
{
    /// <summary>
    /// Интерфейс обратной связи для CommandSession.
    /// </summary>
    public interface CommandSessionCallback
    {
        /// <summary>
        /// Посылает команду на воспроизведения звукового сообщения.
        /// </summary>
        /// <param name="aCommandSession">Сессия клиента, пославшего команду.</param>
        /// <param name="aChannelIds">Идентификаторы выходных каналов для воспроизведения.</param>
        /// <param name="aPriotity">Приоритет.</param>
        /// <param name="aFiles">Файлы для воспроизведения.</param>
        void OnPlaySound(CommandSession aCommandSession, int[] aChannelIds, int aPriotity, string[] aFiles);
    }
}