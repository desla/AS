using System;

namespace Alvasoft.AudioServer.Communication.CommandParsing
{
    /// <summary>
    /// Чтение символов из буфера (строки).
    /// </summary>
    public class CharReader
    {
        private String buffer;
        private int currentIndex;        

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public CharReader()
        {
            buffer = "";
            currentIndex = 0;
        }

        /// <summary>
        /// Конструктор с требуемым буфером.
        /// </summary>
        /// <param name="aBuffer">Буфер.</param>
        public CharReader(String aBuffer)
        {
            SetBuffer(aBuffer);
        }

        /// <summary>
        /// Инициализирует буфер.
        /// </summary>
        /// <param name="aBuffer">Буфер.</param>
        public void SetBuffer(String aBuffer)
        {
            buffer = aBuffer;
            currentIndex = 0;
        }

        /// <summary>
        /// Читает символ.
        /// </summary>
        /// <returns>Текущий прочитанный символ. <code>'\0'</code>, если больше нет.</returns>
        public char ReadChar()
        {
            if (currentIndex > buffer.Length - 1) {
                return '\0';
            }
            char currentChar = buffer[currentIndex];
            currentIndex++;
            return currentChar;
        }

        /// <summary>
        /// Возвращает следующий символ, который будет прочитан.
        /// </summary>
        /// <returns>Символ. <code>'\0'</code>, если больше нет.</returns>
        public char NextChar()
        {
            if (currentIndex > buffer.Length - 1) {
                return '\0';
            }
            return buffer[currentIndex];
        }
    }
}