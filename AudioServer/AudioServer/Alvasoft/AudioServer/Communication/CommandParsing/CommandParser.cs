using System;
using System.Collections.Generic;
using System.Linq;

namespace Alvasoft.AudioServer.Communication.CommandParsing
{
    /// <summary>
    /// Парсер команды.
    /// </summary>
    internal class CommandParser
    {
        private WordReader wordReader;
        private List<String> words;

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public CommandParser()
        {
            wordReader = new WordReader();
            words = new List<String>();
        }

        /// <summary>
        /// Инициализирует команду.
        /// </summary>
        /// <param name="aCommand">Текст команды.</param>
        public void SetCommand(String aCommand)
        {
            words.Clear();
            wordReader.SetBuffer(aCommand);
            String word;
            while ((word = wordReader.ReadWord()) != null) {
                words.Add(word);
            }
        }

        /// <summary>
        /// Возвращает количество слов в команде.
        /// </summary>
        /// <returns>Количество слов.</returns>
        public int GetWordsCount()
        {
            return words.Count();
        }

        /// <summary>
        /// Возвращает слово по указанному индексу.
        /// </summary>
        /// <param name="aIndex">Индекс. <code>null</code>, если нет.</param>
        /// <returns>Слово.</returns>
        public String GetWord(int aIndex)
        {
            if (aIndex > words.Count() - 1) {
                return null;
            }
            return words[aIndex];
        }
    }
}