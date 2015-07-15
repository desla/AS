namespace Alvasoft.Mossner
{
    /// <summary>
    /// Вспомогательный класс для обработки идентификатора слитка.
    /// </summary>
    public static class IdHelper
    {
        /// <summary>
        /// Вытаскивает номер слитка.
        /// </summary>
        /// <param name="aSlabId">Идентификатор слитка.</param>
        /// <returns>Номер слитка.</returns>
        public static int SlabNumber(int aSlabId)
        {
            return aSlabId % 100;
        }

        /// <summary>
        /// Вытаскивает номер плавки.
        /// </summary>
        /// <param name="aSlabId">Идентификатор слитка.</param>
        /// <returns>Номер плавки.</returns>
        public static int MeltNumber(int aSlabId)
        {
            return (aSlabId % 1000000 - aSlabId % 100) / 100;
        }

        /// <summary>
        /// Вытаскивает идентификатор плавки.
        /// </summary>
        /// <param name="aSlabId">Идентификатор слитка.</param>
        /// <returns>Идентификатор плавки.</returns>
        public static int MeltId(int aSlabId)
        {
            return (aSlabId - aSlabId % 100) / 100;
        }

        /// <summary>
        /// Вытаскивает номер миксера.
        /// </summary>
        /// <param name="aSlabId">Идентификатор слитка.</param>
        /// <returns>Номер миксера.</returns>
        public static int MixerNumber(int aSlabId)
        {
            return (aSlabId - aSlabId % 1000000) / 1000000;
        }
    }
}
