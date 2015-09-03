using System;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;

namespace Alvasoft.AudioServer.SoundsStorage.Impl
{
    /// <summary>
    /// Обертка над SpeechSynthesizer для обеспечения потокобезопасности.
    /// </summary>
    public class VoiceSynthesizer
    {
        private SpeechSynthesizer generator; // генератор звука                
        private object generatorLock = new object();
        private SpeechAudioFormatInfo audioFormat; // настраиваемый аудио формат для генерации звука        

        /// <summary>
        /// Класс генератора звука.
        /// </summary>
        /// <param name="aVoice">Выбранный голос из системы.</param>
        /// <param name="aRate">Скорость воспроизведения.</param>
        /// <param name="aSamplePerSecond">Сэмплов в секунду.</param>
        public VoiceSynthesizer(string aVoice, int aRate = 0, int aSamplePerSecond = 22050)
        {
            if (string.IsNullOrEmpty(aVoice)) {
                throw new ArgumentNullException("aVoice");
            }

            if (aRate < -10 || aRate > 10) {
                throw new ArgumentException("Rate must be in the interval [-10; 10]");
            }
            
            generator = new SpeechSynthesizer();            
            generator.SelectVoice(aVoice);
            generator.Rate = aRate;

            audioFormat = new SpeechAudioFormatInfo(aSamplePerSecond, AudioBitsPerSample.Sixteen, AudioChannel.Stereo);
        }

        /// <summary>
        /// Генерирует звук из текста.
        /// </summary>
        /// <param name="aText">Фраза для произношения.</param>
        /// <returns>Сгенерированные звуковые данные.</returns>
        public byte[] Generate(string aText)
        {
            if (string.IsNullOrEmpty(aText)) {
                throw new ArgumentNullException("aText");
            }

            using (var stream = new MemoryStream()) {
                lock (generatorLock) {
                    generator.SetOutputToAudioStream(stream, audioFormat);
                    generator.Speak(aText);
                    generator.SetOutputToNull();
                }
                return stream.ToArray();
            }            
        }

        /// <summary>
        /// Скорость воспроизведения.
        /// </summary>
        /// <returns>Скорость воспроизведения.</returns>
        public int GetRate()
        {
            return generator.Rate;
        }

        /// <summary>
        /// Голос.
        /// </summary>
        /// <returns>Голос.</returns>
        public string GetVoice()
        {
            return generator.Voice.Name;
        }        
    }
}
