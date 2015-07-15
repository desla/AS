using System;

namespace Alvasoft.AudioServer.ChannelsManager
{
    /// <summary>
    /// Сообщение для воспроизведения.
    /// </summary>
    public class SoundMessage
    {
        private byte[] sound;
        private uint priority;

        /// <summary>
        /// Звуковое сообщение
        /// </summary>
        /// <param name="aSourceSound">Данные для воспроизведения.</param>
        /// <param name="aPriority">Приоритет сообщения. 0 - минимальный. Максимальный по-умолчанию не больше 255.</param>
        public SoundMessage(byte[] aSourceSound, uint aPriority)
        {
            if (aSourceSound == null || aSourceSound.Length == 0) {
                throw new ArgumentNullException("aSourceSound");
            }

            sound = new byte[aSourceSound.Length];
            aSourceSound.CopyTo(sound, 0);
            priority = aPriority;
        }

        /// <summary>
        /// Получить данные для воспроизведения.
        /// </summary>
        /// <returns>Данные для воспроизведения.</returns>
        public byte[] GetSound()
        {
            var buffer = new byte[sound.Length];
            sound.CopyTo(buffer, 0);
            return buffer;
        }

        /// <summary>
        /// Получить приоритет сообщения.
        /// </summary>
        /// <returns>Приоритет сообщения</returns>
        public uint GetPriority()
        {
            return priority;
        }
    }
}
