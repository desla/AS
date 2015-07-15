using System;
using System.Collections.Generic;
using System.Linq;

namespace Alvasoft.AudioServer.SoundsStorage
{
    /// <summary>
    /// Хранит информацию о провайдерах звуковых данных.
    /// Порядок их добавления через функцию AddProvider определяет их приоритет.
    /// </summary>
    public class SoundStorage
    {
        private List<SoundProvider> providers = new List<SoundProvider>();

        /// <summary>
        /// "Хранилище" звуковых данных.
        /// </summary>
        /// <param name="aProviders">Список провайдеров.</param>
        public SoundStorage(IEnumerable<SoundProvider> aProviders)
        {
            if (aProviders == null) {
                throw new ArgumentNullException("aProviders");
            }

            foreach (var provider in aProviders) {
                AddProvider(provider);
            }
        }

        /// <summary>
        /// Добавление провайдера к существующим. Приоритет выпора - порядок добавления.
        /// </summary>
        /// <param name="aProviders">Провайдер.</param>
        public void AddProvider(SoundProvider aProviders)
        {
            if (aProviders == null) {
                throw new ArgumentNullException("aProviders");
            }

            providers.Add(aProviders);
        }

        /// <summary>
        /// Найти звуковые данные по ключу (имени файла) у зарегистрированных провайдеров.
        /// </summary>
        /// <param name="aSoundKeys">Ключ (имя файла)</param>
        /// <returns>Звуковые данные.</returns>
        public byte[] ProvideSound(IEnumerable<string> aSoundKeys)
        {
            if (aSoundKeys == null) {
                throw new ArgumentNullException("aSoundKeys");
            }

            var buffer = new byte[0]; // накапливаем звуковое сообщение от разных провайдеров
            foreach (var sound in aSoundKeys) {
                foreach (var provider in providers) {
                    if (provider.IsContainSound(sound)) {
                        var data = provider.ProvideSoundData(sound);
                        buffer = Enumerable.Concat(buffer, data).ToArray();
                        break;
                    }
                }
            }

            return buffer;
        }
    }
}
