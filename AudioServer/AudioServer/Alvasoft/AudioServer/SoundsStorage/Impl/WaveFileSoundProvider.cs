using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Alvasoft.AudioServer.SoundsStorage.Impl
{
    /// <summary>
    /// Провайдер, который загружает звуковые данные из wav-файлов.
    /// </summary>
    public class WaveFileSoundProvider : SoundProvider
    {
        private const int wavFileHeaderSize = 48;
        private HashSet<string> files;
        private List<byte[]> soundsData;

        /// <summary>
        /// Wave-file провайдер.
        /// </summary>
        /// <param name="aFolder">Папка, в которой хранятся все загружаемые файлы.</param>
        public WaveFileSoundProvider(string aFolder)
        {
            if (string.IsNullOrEmpty(aFolder)) {
                throw new ArgumentNullException("aFolder");
            }

            var folderPath = aFolder;
            if (!Path.IsPathRooted(aFolder)) {
                folderPath = Application.StartupPath + "\\" + aFolder;
            }

            LoadFromFolder(folderPath);
        }

        /// <summary>
        /// Содержит ли указанный звук провайдер или нет.
        /// </summary>
        /// <param name="aSource">Ключ (имя файла или фраза для произношения).</param>
        /// <returns></returns>
        public bool IsContainSound(string aSource)
        {
            if (string.IsNullOrEmpty(aSource)) {
                throw new ArgumentNullException("aSource");
            }

            return files.Contains(aSource + ".wav");            
        }

        /// <summary>
        /// Возвращает звуковые данные по ключу (имени файла или фразе для произношения).
        /// </summary>
        /// <param name="aSource">Ключ.</param>
        /// <returns>Звуковые данные.</returns>
        public byte[] ProvideSoundData(string aSource)
        {
            var soundsCound = files.Count;
            var soundKey = aSource + ".wav";
            for (var i = 0; i < soundsCound; ++i) {
                if (soundKey.Equals(files.ElementAt(i))) {
                    var bufferLength = soundsData[i].Length;
                    var buffer = new byte[bufferLength];
                    soundsData[i].CopyTo(buffer, 0);
                    return buffer;
                }
            }

            throw new Exception(string.Format("{0} not found in WaveFileSoundProvider", aSource));
        }

        /// <summary>
        /// Загружает все файлы, находящиеся в указанной папке в память. Ключ - имя файла.
        /// </summary>
        /// <param name="aFolder">Директория с файлами.</param>
        private void LoadFromFolder(string aFolder)
        {
            if (!Directory.Exists(aFolder)) {
                throw new ArgumentException(string.Format("Directody '{0}' is not exist", aFolder));
            }

            files = new HashSet<string>();
            var fullPathFiles = Directory.GetFiles(aFolder);
            foreach (var file in fullPathFiles) {
                files.Add(file.Replace(aFolder + "\\", string.Empty));
            }

            soundsData = new List<byte[]>(files.Count);
            foreach (var file in files) {
                soundsData.Add(LoadDataFromFile(aFolder + "\\" + file));
            }
        }

        /// <summary>
        /// Загружает данные из файла в буфер.
        /// </summary>
        /// <param name="aFileName">Имя файла.</param>
        /// <returns>Данные из файла.</returns>
        private byte[] LoadDataFromFile(string aFileName)
        {
            using (var stream = new FileStream(aFileName, FileMode.Open)) {                
                var dataLength = (int) stream.Length - wavFileHeaderSize;
                if (dataLength < 0) {
                    throw new Exception(string.Format("File {0} has incorrect wav file format", aFileName));
                }

                var data = new byte[dataLength];
                stream.Seek(wavFileHeaderSize, SeekOrigin.Begin);
                stream.Read(data, 0, dataLength);
                return data;
            }
        }        
    }
}
