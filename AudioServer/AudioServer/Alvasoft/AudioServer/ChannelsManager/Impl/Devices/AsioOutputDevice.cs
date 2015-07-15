using System;
using System.Threading;
using NAudio.Wave;

namespace Alvasoft.AudioServer.ChannelsManager.Impl.Devices
{
    /// <summary>
    /// Выходное многоканальное аудио-устройство споддержкой asio-протокола.
    /// </summary>
    public class AsioOutputDevice : IOutputDevice
    {
        private AsioOutExtended asioDriver;
        private int bytePerSample = 2;
        private int samplePerSec = 44100;

        private AsioDeviceChannel[] Channels;
        
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aAsioDriverName">Имя asio-драйвера.</param>
        public AsioOutputDevice(string aAsioDriverName)
        {
            var waveFormat = new WaveFormat(samplePerSec, 2);
            asioDriver = new AsioOutExtended(aAsioDriverName);
            var channelCount = asioDriver.DriverOutputChannelCount;    
            asioDriver.Sources = new IWaveProvider[channelCount];                                      
            Channels = new AsioDeviceChannel[channelCount];            
            for (var i = 0; i < channelCount; ++i) {
                Channels[i] = new AsioDeviceChannel {                    
                    Buffer = new BufferedWaveProvider(waveFormat),                    
                    CurrentLevel = 0,
                    Locker = new object()
                };

                asioDriver.Sources[i] = new StereoToMonoProvider16(Channels[i].Buffer) {
                    RightVolume = 1,
                    LeftVolume = 1
                };
            }

            asioDriver.Init(asioDriver.Sources[0].WaveFormat);
            asioDriver.Play();
        }

        /// <summary>
        /// Возвращает текущий уровень звука на канале.
        /// </summary>
        /// <param name="aChannelId">Номер канала.</param>
        /// <returns>Уровень звука.</returns>
        public int GetCurrentLevel(int aChannelId)
        {
            if (aChannelId < 0 || aChannelId >= Channels.Length) {
                throw new ArgumentException("Номер канала не корректный: " + aChannelId);
            }

            return Channels[aChannelId].CurrentLevel;
        }

        /// <summary>
        /// Устанавливает SamplePerSecond.
        /// </summary>
        /// <param name="aSamplePerSecond">Значение.</param>
        public void SetSamplePerSecond(int aSamplePerSecond)
        {
            throw new NotImplementedException();
            samplePerSec = aSamplePerSecond;            
        }

        /// <summary>
        /// Устанавливает количество байт в сэмпле.
        /// </summary>
        /// <param name="aBytePerSample">Количество байт в сэмпле.</param>
        public void SetBytePerSample(int aBytePerSample)
        {
            throw new NotImplementedException();
            bytePerSample = aBytePerSample;
        }

        /// <summary>
        /// Возвращает количество физических каналов устройства.
        /// </summary>
        /// <returns>Количество каналов.</returns>
        public int GetChannelsCount()
        {
            return Channels.Length;
        }
                
        /// <summary>
        /// Записывает звуковые данные в буфер устройтсва для воспроизведения.
        /// </summary>
        /// <param name="aSoundData">Звуковой буфер.</param>
        /// <param name="aChannelId">Номер канала для записи.</param>
        public void WriteDataToChannel(byte[] aSoundData, int aChannelId)
        {            
            if (aChannelId < 0 || aChannelId >= Channels.Length) {
                throw new ArgumentException("Номер канала не корректный: " + aChannelId);
            }

            var stepSpeed = 100; // миллисекунды
            var bytePerStep = bytePerSample * samplePerSec * stepSpeed / 1000;

            var channel = Channels[aChannelId];            
            lock (channel.Locker) {                
                channel.Buffer.ClearBuffer();
                channel.Buffer.AddSamples(aSoundData, 0, aSoundData.Length);                

                var stepsCount = 10 + channel.Buffer.BufferedDuration.TotalMilliseconds / stepSpeed;
                for (var stepNumber = 0; stepNumber < stepsCount; ++stepNumber) {
                    var dataIndex = stepNumber * bytePerStep;
                    if (dataIndex < aSoundData.Length) {
                        channel.CurrentLevel = aSoundData[dataIndex] << 5;                        
                    }                    
                    Thread.Sleep(stepSpeed);
                }

                channel.CurrentLevel = 0;
            }
        }

        /// <summary>
        /// Освобождает ресурсы.
        /// </summary>
        public void Dispose()
        {
            if (asioDriver != null) {
                asioDriver.Dispose();
            }            
        }
    }
}
