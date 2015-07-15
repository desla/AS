using System;

namespace Alvasoft.AudioServer.SoundsStorage.Impl
{
    /// <summary>
    /// Создает голос из фразы.
    /// </summary>
    public class GeneratorSoundProvider : SoundProvider
    {
        private VoiceSynthesizer generator; // генератор голоса.

        /// <summary>
        /// Создает провайдер, который генерирует голос.
        /// </summary>
        /// <param name="aGenerator">Генератор голоса.</param>
        public GeneratorSoundProvider(VoiceSynthesizer aGenerator)
        {
            if (aGenerator == null) {
                throw new ArgumentNullException("aGenerator");
            }

            generator = aGenerator;
        }

        /// <summary>
        /// Содержит ли указанный звук провайдер или нет.
        /// </summary>
        /// <param name="aText">Ключ (имя файла или фраза для произношения).</param>
        /// <returns>True - если создержит звук, False - если нет.</returns>
        public bool IsContainSound(string aText)
        {
            if (string.IsNullOrEmpty(aText)) {
                throw new ArgumentNullException("aText");
            }

            return true;
        }

        /// <summary>
        /// Генерирует звук из фразы.
        /// </summary>
        /// <param name="aText">Фраза для произношения.</param>
        /// <returns>Звуковые данные.</returns>
        public byte[] ProvideSoundData(string aText)
        {
            return generator.Generate(aText);
        }
    }
}
