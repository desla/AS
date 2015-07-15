using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Alvasoft.AudioServer.SoundsStorage.Impl
{
    /// <summary>
    /// Содержит карту имяФайла-фраза
    /// </summary>
    public class MapSoundProvider : SoundProvider
    {
        private const string NODE_ITEM = "item";
        private const string NODE_FILENAME = "filename";
        private const string NODE_MESSAGE = "message";

        private VoiceSynthesizer generator;
        private HashSet<string> fileNames = new HashSet<string>();
        private List<string> phrases = new List<string>();

        /// <summary>
        /// Создает провайдер-карту фраз по ключу.
        /// </summary>
        /// <param name="aGenerator">Генератор голоса.</param>
        /// <param name="aXmlFile">Файл-карта.</param>
        public MapSoundProvider(VoiceSynthesizer aGenerator, string aXmlFile)
        {
            if (aGenerator == null) {
                throw new ArgumentNullException("aGenerator");
            }

            if (string.IsNullOrEmpty(aXmlFile)) {
                throw new ArgumentNullException("aXmlFile");
            }

            var filePath = aXmlFile;
            if (!Path.IsPathRooted(aXmlFile)) {
                filePath = Application.StartupPath + "\\" + aXmlFile;
            }

            generator = aGenerator;
            LoadFromXmlFile(filePath);
        }

        /// <summary>
        /// Загружает карту фраз из указанного файла.
        /// </summary>
        /// <param name="aXmlFile">XML Файл карта ключ-фраза.</param>
        private void LoadFromXmlFile(string aXmlFile)
        {            
            var document = new XmlDocument();
            document.Load(aXmlFile);
                            
            var root = document.DocumentElement;

            if (root == null) {
                throw new XmlException("File " + aXmlFile + " is not contain root element");
            }            

            var items = root.ChildNodes;
            for (var itemIndex = 0; itemIndex < items.Count; ++itemIndex) {
                switch (items[itemIndex].Name) {
                    case NODE_ITEM: {
                            var filename = string.Empty;
                            var message = string.Empty;
                            var item = items[itemIndex];
                            var itemFiels = item.ChildNodes;
                            for (var fieldIndex = 0; fieldIndex < itemFiels.Count; ++fieldIndex) {
                                switch (itemFiels[fieldIndex].Name) {
                                    case NODE_FILENAME:
                                        filename = itemFiels[fieldIndex].InnerText.Trim();
                                        break;
                                    case NODE_MESSAGE:
                                        message = itemFiels[fieldIndex].InnerText.Trim();
                                        break;
                                }
                            }

                            AddPhrase(filename, message);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Добавляет значение в карту.
        /// </summary>
        /// <param name="aFileName">Имя файла.</param>
        /// <param name="aText">Соответствующая fileName'у (ключу) фраза.</param>
        private void AddPhrase(string aFileName, string aText)
        {
            if (string.IsNullOrEmpty(aFileName)) {
                throw new ArgumentNullException("aFileName");
            }

            if (string.IsNullOrEmpty(aText)) {
                throw new ArgumentNullException("aText");
            }

            if (IsContainSound(aFileName)) {
                throw new ArgumentException(string.Format("Phrase for {0} already exist", aFileName));
            }

            fileNames.Add(aFileName);
            phrases.Add(aText);
        }

        /// <summary>
        /// Содержит ли указанный звук провайдер или нет.
        /// </summary>
        /// <param name="aFileName">Ключ (имя файла или фраза для произношения).</param>
        /// <returns>True - если создержит звук, False - если нет.</returns>
        public bool IsContainSound(string aFileName)
        {
            if (string.IsNullOrEmpty(aFileName)) {
                throw new ArgumentNullException("aFileName");
            }

            return fileNames.Contains(aFileName);
        }

        /// <summary>
        /// Возвращает звуковые данные по ключу (имени файла или фразе для произношения).
        /// </summary>
        /// <param name="aFileName">Ключ.</param>
        /// <returns>Звуковые данные.</returns>
        public byte[] ProvideSoundData(string aFileName)
        {
            if (string.IsNullOrEmpty(aFileName)) {
                throw new ArgumentNullException("aFileName");
            }

            var soundsCount = fileNames.Count;
            for (var i = 0; i < soundsCount; ++i) {
                if (aFileName.Equals(fileNames.ElementAt(i))) {
                    return generator.Generate(phrases[i]);
                }
            }

            throw new Exception(string.Format("{0} not found in MapSoundProvider", aFileName));
        }
    }
}
