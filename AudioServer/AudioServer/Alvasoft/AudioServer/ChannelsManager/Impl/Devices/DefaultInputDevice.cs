using System;
using System.Runtime.InteropServices;
using System.Threading;
using AudioServer.Alvasoft.AudioServer.ChannelsManager.Impl.Devices;
using AudioServer.WinMM.DllImports;
using AudioServer.WinMM.Structures;

namespace Alvasoft.AudioServer.ChannelsManager.Impl.Devices
{
    /// <summary>
    /// Устройство ввода.
    /// </summary>
    public class DefaultInputDevice : IInputDevice
    {
        private int seconds = 2 * 60; // максимальное время записи одного сообщения.            
        private int samplePerSec = 22050; // Сэмплов в секунду
        private int bytePerSample = 2; // байт в сэмпле        
        private IntPtr handle = IntPtr.Zero;        
        private object stopRecordLock = new object(); // для потокобезопасного изменения текущего состояния.
        private bool isBusy = false;
        private string deviceName;
        private int systemId; // номер устройства в системе
        private int channelsCount;
        
        private int currentInChannelId;
        private int[] currentOutChannelsIds;
        private WAVEHDR currentWaveInHdr;
        private int currentRecordOwner = -1;
        private Thread recordControlThread;

        private IChannelManager callback;

        /// <summary>
        /// Выполняет поиск физического устройства ввода.
        /// </summary>
        /// <param name="aDeviceName">Имя устройства.</param>
        /// <returns>Устройство ввода или null.</returns>
        public static DefaultInputDevice FindPhysicalDeviceByName(string aDeviceName)
        {
            int channelCount;
            int deviceId;
            if (!IsDeviceFound(aDeviceName, out deviceId, out channelCount)) {
                return null;
            }

            return new DefaultInputDevice(aDeviceName, deviceId, channelCount);
        }    

        /// <summary>
        /// Устанавливает callback для обратных вызовов.
        /// </summary>
        /// <param name="aCallback">Callback.</param>
        public void SetCallback(IChannelManager aCallback)
        {
            callback = aCallback;
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aDeviceName">Имя устройства.</param>
        /// <param name="aDeviceId">Идентификатор.</param>
        /// <param name="aChannelCount">Количество каналов.</param>
        private DefaultInputDevice(string aDeviceName, int aDeviceId, int aChannelCount)
        {
            deviceName = aDeviceName;
            channelsCount = aChannelCount;
            systemId = aDeviceId;            

            if (OpenDevice(aDeviceId) != 0) {
                throw new InvalidOperationException(
                    string.Format("Не удалось открыть устройство с идентификатором: {0}", aDeviceId));
            }            
        }

        /// <summary>
        /// Контролирует продолжительность записи сообщения.
        /// Прекращает запись по истечении указанного времени.
        /// </summary>
        /// <param name="aInChannelId"></param>
        /// <param name="aRecordOwner"></param>
        private void RecordControlMethod(int aInChannelId, int aRecordOwner)
        {            
            Thread.Sleep(seconds * 1000);            
            StopRecord(currentInChannelId, aRecordOwner);
        }

        /// <summary>
        /// Получает handle для физического устройства.
        /// </summary>
        /// <param name="aDeviceId">Идентификатор устройства в системе.</param>
        /// <returns>Результат. Если != 0, значит ошибка.</returns>
        private int OpenDevice(int aDeviceId)
        {
            var format = new WAVEFORMATEX {
                WFormatTag = (short)WAVEFORMATTAG.WAVE_FORMAT_PCM,
                NChannels = (short)channelsCount,
                NSamplesPerSec = samplePerSec,
                NAvgBytesPerSec = samplePerSec * bytePerSample * channelsCount,
                NBlockAlign = (short)(bytePerSample * channelsCount),
                WBitsPerSample = (short)(8 * bytePerSample),
                CbSize = 0
            };

            return (int)NativeMethods.waveInOpen(ref handle, (uint)aDeviceId, ref format, 
                IntPtr.Zero, 0, (uint)WAVEINOPENFLAGS.WAVE_FORMAT_DIRECT);
        }

        /// <summary>
        /// Выполняет поиск физического устройства в системе.
        /// </summary>
        /// <param name="aDeviceName">Имя устройства.</param>
        /// <param name="aDeviceNumber">Выходной параметр - номер устройства в системе.</param>
        /// <param name="aChannelCount">Выходной параметр - количество каналов.</param>
        /// <returns>True - если устройство найдено, false - иначе.</returns>
        private static bool IsDeviceFound(string aDeviceName, out int aDeviceNumber, out int aChannelCount)
        {
            var deviceCount = NativeMethods.waveInGetNumDevs();

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
        /// Получает найтсройки физического устройства.
        /// </summary>
        /// <param name="aDeviceId">Идентификатор устройства.</param>
        /// <param name="aDeviceName">Выходной параметр - имя устройства.</param>
        /// <param name="aChannelCount">Выходной параметр - количество каналов.</param>
        private static void GetDeviceCaps(int aDeviceId, out string aDeviceName, out int aChannelCount)
        {
            var wInCaps = new WAVEINCAPS();
            NativeMethods.waveInGetDevCaps(new UIntPtr((uint)aDeviceId),
                                           ref wInCaps,
                                           (uint)Marshal.SizeOf(typeof(WAVEINCAPS)));

            aDeviceName = wInCaps.SzPname;
            aChannelCount = wInCaps.WChannels;
        }

        /// <summary>
        /// Возвращает количество каналов на устройстве.
        /// </summary>
        /// <returns></returns>
        public int GetChannelsCount()
        {
            return channelsCount;
        }

        /// <summary>
        /// Начинает запись звука на канале.
        /// </summary>
        /// <param name="aInChannelNumber">Номер канала.</param>
        /// <param name="aOutChannelsIds">Идентификаторы выходных каналов.</param>
        /// <param name="aRecordOwner">Инициатор записи.</param>
        public void StartRecord(int aInChannelNumber, int[] aOutChannelsIds, int aRecordOwner)
        {
            lock (stopRecordLock) {
                if (isBusy) {
                    return;
                }

                isBusy = true;

                var bytePerSecond = samplePerSec*bytePerSample*channelsCount;

                var bufferLength = seconds*bytePerSecond;                

                currentWaveInHdr = new WAVEHDR {
                    LpData = Marshal.AllocHGlobal(bufferLength),
                    DwBufferLength = (uint) (bufferLength),
                    DwBytesRecorded = 0,
                    DwUser = IntPtr.Zero,
                    DwFlags = 0,
                    DwLoops = 0
                };

                NativeMethods.waveInPrepareHeader(handle, ref currentWaveInHdr, 
                    (uint) Marshal.SizeOf(typeof (WAVEHDR)));

                currentInChannelId = aInChannelNumber;
                currentOutChannelsIds = aOutChannelsIds;
                currentRecordOwner = aRecordOwner;

                recordControlThread = new Thread(() => RecordControlMethod(aInChannelNumber, aRecordOwner));
                recordControlThread.Start();

                NativeMethods.waveInAddBuffer(handle, ref currentWaveInHdr, 
                    (uint) Marshal.SizeOf(typeof (WAVEHDR)));
                NativeMethods.waveInStart(handle);
            }
        }

        /// <summary>
        /// Заканчивает запись звука на канале.
        /// </summary>
        /// <param name="aChannelNumber">Идентификатор канала.</param>
        /// <param name="aRecordOwner">Инициатор записи.</param>
        public void StopRecord(int aChannelNumber, int aRecordOwner)
        {
            lock (stopRecordLock) {
                if (!isBusy) {
                    return;
                }

                if (aRecordOwner != currentRecordOwner) {
                    return;
                }

                if (Thread.CurrentThread != recordControlThread) {
                    recordControlThread.Abort();                    
                }

                currentRecordOwner = -1;
                isBusy = false;

                NativeMethods.waveInReset(handle);                                        

                if (currentWaveInHdr.DwBytesRecorded == 0) {                    
                    return;
                }

                var buffer = new byte[currentWaveInHdr.DwBytesRecorded];
                Marshal.Copy(currentWaveInHdr.LpData, buffer, 0, buffer.Length);
                Marshal.FreeHGlobal(currentWaveInHdr.LpData);
                currentWaveInHdr.DwBytesRecorded = 0;

                var outChannelsIds = new int[currentOutChannelsIds.Length];
                currentOutChannelsIds.CopyTo(outChannelsIds, 0);
                
                CopySoundOnAllChannels(buffer, aChannelNumber);

                if (callback != null) {
                    var message = new SoundMessage(buffer, 255);
                    foreach (var channelId in outChannelsIds) {
                        callback.ProcessMessage(message, channelId);
                    }
                }
            }
        }

        /// <summary>
        /// Множит звук из указанного канала на все остальные.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="aChannelNumber"></param>
        private void CopySoundOnAllChannels(byte[] buffer, int aChannelNumber)
        {
            var sampleCount = buffer.Length/(bytePerSample*channelsCount);
            for (var i = 0; i < sampleCount; ++i) {
                var sampleCopy = new byte[bytePerSample];
                var sampleBlockIndex = i * bytePerSample * channelsCount;
                var sampleIndex = sampleBlockIndex + bytePerSample*aChannelNumber;
                for (var j = 0; j < bytePerSample; ++j) {
                    sampleCopy[j] = buffer[sampleIndex + j];
                }

                for (var j = 0; j < channelsCount; ++j) {
                    var currentSampleIndex = sampleBlockIndex + bytePerSample * j;
                    for (var k = 0; k < bytePerSample; ++k) {
                        buffer[currentSampleIndex + k] = sampleCopy[k];
                    }
                }
            }
        }

        /// <summary>
        /// Освобождает ресурсы.
        /// </summary>
        public void Dispose()
        {
            if (handle != IntPtr.Zero) {
                NativeMethods.waveInClose(handle);
            }
        }      
    }
}
