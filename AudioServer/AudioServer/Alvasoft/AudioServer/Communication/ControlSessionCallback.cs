namespace Alvasoft.AudioServer.Communication
{
    /// <summary>
    /// Интерфейс обратных вызовов сессии управления.
    /// </summary>
    public interface ControlSessionCallback
    {
        /// <summary>
        /// Срабатывает при потери соединения.
        /// </summary>
        /// <param name="aControlSession">Сессия клиента.</param>
        void OnCloseConnection(ControlSession aControlSession);

        /// <summary>
        /// Срабатывает при запросе количества управляющих соединений на сервере.
        /// </summary>
        /// <param name="aControlSession">Сессия клиента.</param>
        /// <param name="aSessionCount">Выходной параметр - количество сессий.</param>
        void OnQueryStatus(ControlSession aControlSession, out int aSessionCount);
        
        /// <summary>
        /// Срабатывает при запросе клиентом информации о каналах.
        /// </summary>
        /// <param name="aControlSession">Сессия клиента.</param>
        /// <param name="aChannelIds">Выходной параметр - идентификаторы каналов.</param>
        /// <param name="aChannelNames">Выходной параметр - заголовки каналов.</param>
        void OnChannelInfo(ControlSession aControlSession, out int[] aChannelIds, out string[] aChannelNames);
        
        /// <summary>
        /// Срабатывает при запросе клиентом информации о входных каналах.
        /// </summary>
        /// <param name="aControlSession">Сессия клиента.</param>
        /// <param name="aInputLineIds">Выходной параметр - идентификаторы входных каналов.</param>
        /// <param name="aInputLineNames">Выходной параметр - заголовки входных каналов.</param>
        void OnInputLine(ControlSession aControlSession, out int[] aInputLineIds, out string[] aInputLineNames);

        /// <summary>
        /// Срабатывает при запросе клиентом информации о каналах.
        /// </summary>
        /// <param name="aControlSession">Сессия клиента.</param>
        /// <param name="aChannelIds">Выходной параметр - идентификаторы выходных каналов.</param>
        /// <param name="aChannelLevels">Выходной параметр - текущие уровни звука на каналах.</param>
        void OnChannelData(ControlSession aControlSession, out int[] aChannelIds, out int[] aChannelLevels);

        /// <summary>
        /// Срабатывает при получении от клиента команды завершения работы сервера.
        /// </summary>
        /// <param name="aControlSession">Сессия клиента.</param>
        void OnShutdown(ControlSession aControlSession);

        /// <summary>
        /// Срабатывает при получении от клиента команды на запись звукового собщения.
        /// </summary>
        /// <param name="aControlSession">Сессия клиента.</param>
        /// <param name="aInputLineId">Идентификатор входного канала.</param>
        /// <param name="aChannelsIds">Идетификаторы выходных каналов.</param>
        void OnStartRecordSound(ControlSession aControlSession, int aInputLineId, int[] aChannelsIds);

        /// <summary>
        /// Срабатывает при получении от клиента команды на прекращения записи голосового сообщения.
        /// </summary>
        /// <param name="aControlSession">Сессия клиента.</param>
        void OnStopRecordSound(ControlSession aControlSession);
    }
}