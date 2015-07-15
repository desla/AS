namespace Alvasoft.AudioServer.Communication.CommandParsing
{
    /// <summary>
    /// Класс для чтения команды.
    /// </summary>
    public class LineReader
    {
        private CharReader charReader = new CharReader();
        private string currentLine = string.Empty;        

        /// <summary>
        /// Добавляет буфер.
        /// </summary>
        /// <param name="aBuffer"></param>
        public void AddBuffer(string aBuffer)
        {            
            charReader.SetBuffer(currentLine + aBuffer);
            currentLine = string.Empty;                        
        }

        /// <summary>
        /// Читает строку из буфера.
        /// </summary>
        /// <returns>Прочитанная команда.</returns>
        public string ReadLine()
        {
            while (charReader.NextChar() != '\0') {
                var currentChar = charReader.ReadChar();

                if ((currentChar == '\r') && (charReader.NextChar() == '\n')) {
                    // Конец строки.
                    // Инорируем уже найденный '\n'.
                    charReader.ReadChar();

                    var line = string.Copy(currentLine);
                    currentLine = string.Empty;

                    return line;
                }
                currentLine += currentChar;
            }
            return null;             
        }
    }
}