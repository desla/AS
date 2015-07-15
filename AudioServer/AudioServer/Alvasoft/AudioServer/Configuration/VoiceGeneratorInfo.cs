namespace Alvasoft.AudioServer.Configuration
{
    /// <summary>
    /// Конфигурация для генератора голоса.
    /// </summary>
    public class VoiceGeneratorInfo
    {
        /// <summary>
        /// Выбранный голос.
        /// </summary>
        public string Voice { get; set; }

        /// <summary>
        /// Скорость произношения.
        /// </summary>
        public int Rate { get; set; }

        /// <summary>
        /// Семплов в секунду. По-умолчания должно быть 22050.
        /// </summary>
        public int SamplePerSecond { get; set; }
    }
}
