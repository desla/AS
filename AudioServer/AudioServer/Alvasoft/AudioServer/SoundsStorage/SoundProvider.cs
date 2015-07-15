namespace Alvasoft.AudioServer.SoundsStorage
{
    /// <summary>
    /// Интерфейс звукового провайдера.
    /// </summary>
    public interface SoundProvider
    {
        /// <summary>
        /// Содержит ли указанный звук провайдер или нет.
        /// </summary>
        /// <param name="aSource">Ключ (имя файла или фраза для произношения).</param>
        /// <returns>True - если создержит звук, False - если нет.</returns>
        bool IsContainSound(string aSource);

        /// <summary>
        /// Возвращает звуковые данные по ключу (имени файла или фразе для произношения).
        /// </summary>
        /// <param name="aSource">Ключ.</param>
        /// <returns>Звуковые данные.</returns>
        byte[] ProvideSoundData(string aSource);        
    }    
}
