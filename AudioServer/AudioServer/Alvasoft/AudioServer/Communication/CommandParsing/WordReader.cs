using System;

namespace Alvasoft.AudioServer.Communication.CommandParsing
{
    /// <summary>
    /// Чтение слов из буфера.
    /// </summary>
    public class WordReader
    {
        private CharReader charReader;

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public WordReader()
        {
            charReader = new CharReader();
        }

        /// <summary>
        /// Конструктор с требуемым буфером.
        /// </summary>
        /// <param name="aBuffer">Буфер.</param>
        public WordReader(String aBuffer)
        {
            SetBuffer(aBuffer);
        }

        /// <summary>
        /// Инициализирует буфер.
        /// </summary>
        /// <param name="aBuffer">Буфер.</param>
        public void SetBuffer(String aBuffer)
        {
            charReader.SetBuffer(aBuffer);
        }

        /// <summary>
        /// Читает слово.
        /// </summary>
        /// <returns>Прочитанное слово. <code>null</code>, если больше нет.</returns>
        public String ReadWord()
        {
            String word = ""; // Текущее формируемое слово.

            // Игнорируем пробелы в начале слова.
            while (charReader.NextChar() == ' ') {
                // Logger.Info( "Игнорирование пробела в начале" );

                // Читаем следующий символ.
                charReader.ReadChar();
            }

            char currentChar;
            while ((currentChar = charReader.NextChar()) != '\0') {
                // Logger.Info( "Символ '{0}\'", currentChar );

                if (word == "") {
                    // Особый случай: запятая, идущая первой.
                    if (currentChar == ',') {
                        word += currentChar;
                        // Читаем следующий символ.
                        charReader.ReadChar();
                        break;
                    }
                }

                if ((currentChar == ' ') || (currentChar == ',')) {
                    // Конец слова.
                    break;
                }

                word += currentChar;
                // Читаем следующий символ.
                charReader.ReadChar();
            }

            if (word == "") {
                return null;
            }
            // Logger.Info( "Слово \"{0}\"", word );
            return word;
        }
    }
}