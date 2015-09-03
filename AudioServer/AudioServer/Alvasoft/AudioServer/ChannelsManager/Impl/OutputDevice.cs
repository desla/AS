using System;
using System.Runtime.InteropServices;
using System.Threading;
using AudioServer.WinMM.DllImports;
using AudioServer.WinMM.Structures;

namespace Alvasoft.AudioServer.ChannelsManager.Impl
{
    /// <summary>
    /// Физическое устройство - звуковая карта
    /// </summary>
    public class OutputDevice : IDisposable
    {
        private int samplePerSec = 22050; // Сэмплов в секунду
        private int bytePerSample = 2; // байт в сэмпле        
        private IntPtr handle = IntPtr.Zero;
        private object writingLock = new object(); // для потокобезопасности воспроизведения
        private string deviceName;
        private int systemId; // номер устройства в системе
        private int channelsCount;
        private int[] channelLevels;

        /// <summary>
        /// Метод возвращает null, если в системе нет устройства с указанным именем.
        /// Если устройтсво есть, тогда конфигурирует OutputDevice используя параметры реального устройства.
        /// </summary>
        /// <param name="aDeviceName">Имя устройства в системе.</param>
        /// <returns>Сконфигурированное устройство.</returns>
        public static OutputDevice FindPhysicalDeviceByName(string aDeviceName)
        {
            int channelCount;
            int deviceId;
            if (!IsDeviceFound(aDeviceName, out deviceId, out channelCount)) {
                return null;
            }            

            return new OutputDevice(aDeviceName, deviceId, channelCount);
        }
        
        /// <summary>
        /// Возвращает текущий уровень звука на канале.
        /// </summary>
        /// <param name="channelId">Номер канала.</param>
        /// <returns>Уровень звука.</returns>
        public int GetCurrentLevel(int channelId)
        {
            if (channelId < 0 || channelId >= channelsCount) {
                throw new ArgumentException("channelId");
            }

            return channelLevels[channelId];
        }

        /// <summary>
        /// Устанавливает количество сэмплов в секунду - параметры воспроизведения.
        /// По-умолчанию 22050.
        /// </summary>
        /// <param name="aSamplePerSecond">Количество сэмплов в секунду.</param>
        public void SetSamplePerSecond(int aSamplePerSecond)
        {
            if (aSamplePerSecond < 1) {
                throw new ArgumentException("aSamplePerSecond must be more than 1");
            }

            samplePerSec = aSamplePerSecond;
        }

        /// <summary>
        /// Устанавливает количество байт в сэмпле - параметры воспроизведения.
        /// По-умолчанию 2.
        /// </summary>
        /// <param name="aBytePerSample">Количество байт в сэмпле.</param>
        public void SetBytePerSample(int aBytePerSample)
        {
            if (aBytePerSample < 1) {
                throw new ArgumentException("aBytePerSample must be more than 1");
            }

            bytePerSample = aBytePerSample;
        }

        /// <summary>
        /// Закрыть звуковое устройство.
        /// </summary>
        public void Dispose()
        {
            if (this.handle != IntPtr.Zero) {
                NativeMethods.waveOutClose(this.handle);
            }
        }

        /// <summary>
        /// Возвращает количество каналов на устройстве.
        /// </summary>
        /// <returns>Количество каналов на устройстве.</returns>
        public int GetChannelsCount()
        {
            return channelsCount;
        }

        /// <summary>
        /// Записываем звук в буфер устройства.
        /// </summary>
        /// <param name="aSoundData">Записываемые данные.</param>
        /// <param name="aChannelId">Номер канала воспроизведения. Если -1 => воспроизводим на всех каналах.</param>
        public void WriteDataToChannel(byte[] aSoundData, int aChannelId)
        {
            if (aSoundData == null) {
                throw new ArgumentNullException("aSoundData");
            }

            if (aChannelId < -1 || aChannelId >= channelsCount) {
                throw new ArgumentException(string.Format("ChannelId={0} is out of range [0 - {1}]", aChannelId, channelsCount - 1));
            }

            var dataLength = aSoundData.Length;

            var soundDataCopy = new byte[dataLength];
            aSoundData.CopyTo(soundDataCopy, 0);

            if (aChannelId != -1) {
                ForbidChannelsExcept(soundDataCopy, aChannelId);
            }

            var pSoundData = Marshal.AllocHGlobal(dataLength);

            try {                
                Marshal.Copy(soundDataCopy, 0, pSoundData, dataLength);

                var waveHdr = new WAVEHDR {
                    LpData = pSoundData,
                    DwBufferLength = (uint) dataLength,
                    DwFlags = 0,
                    DwLoops = 0
                };

                lock (writingLock) {
                    var stepNumber = 0;
                    var stepSpeed = 100; // миллисекунды
                    var bytePerStep = 2*bytePerSample*samplePerSec*stepSpeed/1000;

                    NativeMethods.waveOutPrepareHeader(this.handle, ref waveHdr, (uint) Marshal.SizeOf(typeof (WAVEHDR)));
                    NativeMethods.waveOutWrite(this.handle, ref waveHdr, (uint) Marshal.SizeOf(typeof (WAVEHDR)));

                    // ожидаем окончания воспроизведения и меняем текущий уровень на каналах
                    while ((waveHdr.DwFlags & WAVEHDRFLAGS.WHDR_DONE) == 0) {
                        var dataIndex = stepNumber*bytePerStep + aChannelId*2;
                        if (dataIndex < dataLength) {
                            var value = soundDataCopy[dataIndex] << 5;
                            channelLevels[aChannelId] = value;
                            //Logger.Info(value);
                            stepNumber++;
                        }

                        Thread.Sleep(stepSpeed);
                    }

                    channelLevels[aChannelId] = 0;

                    NativeMethods.waveOutUnprepareHeader(this.handle, ref waveHdr,
                        (uint) Marshal.SizeOf(typeof (WAVEHDR)));
                    NativeMethods.waveOutReset(this.handle);
                }
            }
            finally {
                // Освободим неуправляемую память
                Marshal.FreeHGlobal(pSoundData);
            }            
        }

        /// <summary>
        /// Конструктор. Приватный, так как параметры должны браться из системы.
        /// </summary>
        /// <param name="aDeviceName">Название устройства.</param>
        /// <param name="aDeviceId">Номер устройства в системе.</param>
        /// <param name="aChannelCount">Количество каналов устройства.</param>
        private OutputDevice(string aDeviceName, int aDeviceId, int aChannelCount)
        {
            deviceName = aDeviceName;
            channelsCount = aChannelCount;
            systemId = aDeviceId;
            channelLevels = new int[channelsCount];

            if (this.OpenDevice(aDeviceId) != 0) {
                throw new InvalidOperationException(string.Format("Не удалось открыть устройство с идентификатором: {0}", aDeviceId));
            }
        }

        /// <summary>
        /// Запретить все каналы, кроме указанного.
        /// </summary>
        /// <param name="aSoundData">Исходные звуковые данные.</param>
        /// <param name="aChannelId">Идентификатор канала.</param>
        private void ForbidChannelsExcept(byte[] aSoundData, int aChannelId)
        {
            for (var i = 0; i < channelsCount; ++i) {
                if (i != aChannelId) {
                    ForbidChannel(aSoundData, i);
                }
            }
        }

        /// <summary>
        /// Запретить указанный канал.
        /// </summary>
        /// <param name="aSoundData">Исходные звуковые данные.</param>
        /// <param name="aChannelId">Идентификатор канала.</param>
        private void ForbidChannel(byte[] aSoundData, int aChannelId)
        {
            // обнуляем байты которые идут на выбранный канал
            var maxLength = aSoundData.Length - aChannelId * channelsCount - 1;
            for (var position = 0; position < maxLength; position += bytePerSample * channelsCount) {
                aSoundData[position + aChannelId*channelsCount] = 0;
                aSoundData[position + aChannelId*channelsCount + 1] = 0;
            }
        }                       

        /// <summary>
        /// Открывает устройство и запоминает его Handle.
        /// </summary>
        /// <param name="aDeviceId">Идентификатор устройства.</param>
        /// <returns>Если != 0 то произошла ошибка.</returns>
        private int OpenDevice(int aDeviceId)
        {
            var format = new WAVEFORMATEX {
                WFormatTag = (short)WAVEFORMATTAG.WAVE_FORMAT_PCM,
                NChannels = (short)channelsCount,
                NSamplesPerSec = samplePerSec,
                NAvgBytesPerSec = samplePerSec * bytePerSample * channelsCount,
                WBitsPerSample = (short)(8 * bytePerSample),
                NBlockAlign = (short)(bytePerSample * channelsCount),
                CbSize = 0
            };

            return (int)NativeMethods.waveOutOpen(ref this.handle, (uint)aDeviceId, ref format, null, (IntPtr)0, 0);
        }

        /// <summary>
        /// Выполняет поиск физического устройства в системе с указанным именем. 
        /// Если устройство найдено, тогда заполняет его системный номер и количество каналов.
        /// </summary>
        /// <param name="aDeviceName">Имя устройства для поиска.</param>
        /// <param name="aDeviceNumber">Номер устройсва.</param>
        /// <param name="aChannelCount">Количество каналов на устройстве.</param>
        /// <returns>True - если устройство найдено, False - иначе.</returns>
        private static bool IsDeviceFound(string aDeviceName, out int aDeviceNumber, out int aChannelCount)
        {
            var deviceCount = NativeMethods.waveOutGetNumDevs();

            for (var deviceId = 0; deviceId < deviceCount; ++deviceId) {
                string foundedDeviceName;
                GetDeviceCaps(deviceId, out foundedDeviceName, out aChannelCount);

                if (aDeviceName.Equals(foundedDeviceName)) {
                    aDeviceNumber = deviceId;
                    return true;
                }
            }

            aDeviceNumber = -1;
            aChannelCount = 0;
            return false;
        }

        /// <summary>
        /// Получить описание системного устройства.
        /// </summary>
        /// <param name="aDeviceId">Идентификатор устройства.</param>
        /// <param name="aDeviceName">Название устройства.</param>
        /// <param name="aChannelCount">Количество каналов устройства.</param>
        private static void GetDeviceCaps(int aDeviceId, out string aDeviceName, out int aChannelCount)
        {
            var wOutCaps = new WAVEOUTCAPS();
            NativeMethods.waveOutGetDevCaps(new UIntPtr((uint)aDeviceId), 
                                            ref wOutCaps, 
                                            (uint)Marshal.SizeOf(typeof(WAVEOUTCAPS)));

            aDeviceName = wOutCaps.SzPname;
            aChannelCount = wOutCaps.WChannels;
        }        
    }
}
