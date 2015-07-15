using System;
using System.IO;
using System.Windows.Forms;
using log4net;

namespace Alvasoft.Utils
{
    /// <summary>
    /// Звуковой префик, который воспроизводится перед сообщением.
    /// </summary>
    public class PrefixSound
    {
        private static readonly ILog Logger = LogManager.GetLogger("SoundPrefix");
        
        /// <summary>
        /// Стандартный размер заголовка wav-файла.
        /// </summary>
        private const int WAV_FILE_HEADER_SIZE = 48;
        private byte[] soundData = null;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aPrefixFileName">Имя файла.</param>
        public PrefixSound(string aPrefixFileName)
        {
            if (string.IsNullOrEmpty(aPrefixFileName)) {
                throw new ArgumentNullException("aPrefixFileName");
            }

            var fullFilePath = aPrefixFileName;
            if (!Path.IsPathRooted(aPrefixFileName)) {
                fullFilePath = Application.StartupPath + "\\" + aPrefixFileName;
            }

            LoadSound(fullFilePath);
        }

        /// <summary>
        /// Возвращает звуковые данные префикса.
        /// </summary>
        /// <returns>Данные.</returns>
        public byte[] GetPrefixSound()
        {
            var soundDataCopy = new byte[soundData.Length];
            soundData.CopyTo(soundDataCopy, 0);
            return soundData;
        }

        private void LoadSound(string aFileName)
        {
            using (var stream = new FileStream(aFileName, FileMode.Open)) {
                var dataLength = (int)stream.Length - WAV_FILE_HEADER_SIZE;
                if (dataLength < 0) {
                    throw new Exception(string.Format("File {0} has incorrect wav file format", aFileName));
                }

                soundData = new byte[dataLength];
                stream.Seek(WAV_FILE_HEADER_SIZE, SeekOrigin.Begin);
                stream.Read(soundData, 0, dataLength);                
            }
        }
    }
}
