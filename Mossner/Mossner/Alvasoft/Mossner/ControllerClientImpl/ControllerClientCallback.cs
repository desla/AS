namespace Alvasoft.Mossner.ControllerClientImpl
{
    public interface ControllerClientCallback
    {
        /// <summary>
        /// Входная область: Идентификация номера слитка.
        /// </summary>
        /// <param name="aControllerClient">Клиент контроллера.</param>
        /// <param name="aSlabId">Номер слитка.</param>
        void ReadSlabIdCheckForInputArea(ControllerClient aControllerClient, int aSlabId);

        /// <summary>
        /// Область обрези: Идентификация номера слитка.
        /// </summary>
        /// <param name="aControllerClient">Клиент контроллера.</param>
        /// <param name="aSlabId">Номер слитка.</param>
        /// <param name="aSlabWeight">Вес слитка.</param>
        /// <param name="aSlabLength">Длина слитка.</param>
        void ReadSlabIdCheckForScrabArea(ControllerClient aControllerClient, int aSlabId, int aSlabWeight, int aSlabLength);

        /// <summary>
        /// Выходная область: Идентификация номера слитка.
        /// </summary>
        /// <param name="aControllerClient">Клиент контроллера.</param>
        /// <param name="aSlabId">Номер слитка.</param>
        /// <param name="aSlabWeight">Вес слитка.</param>
        /// <param name="aSlabLength">Длина слитка.</param>
        void ReadSlabIdCheckForOutputArea(ControllerClient aControllerClient, int aSlabId, int aSlabWeight, int aSlabLength);
    }
}
