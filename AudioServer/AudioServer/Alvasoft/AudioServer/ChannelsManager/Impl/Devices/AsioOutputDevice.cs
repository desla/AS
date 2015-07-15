using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAudio.Wave;

namespace Alvasoft.AudioServer.ChannelsManager.Impl.Devices
{
    /// <summary>
    /// Выходное многоканальное аудио-устройство споддержкой asio-протокола.
    /// </summary>
    public class AsioOutputDevice : IOutputDevice
    {
        private int bytePerSample = 2;
        private int samplePerSec = 44100;

        public AsioOut driver;
        public BufferedWaveProvider buffer;
        public int currentLevel;
        public object locker = new object();
        
        public AsioOutputDevice(string aAsioDriverName)
        {
            var waveFormat = new WaveFormat(samplePerSec, 2);            

            driver = new AsioOut(aAsioDriverName);            
            buffer = new BufferedWaveProvider(waveFormat);
            var monoBuffer = new StereoToMonoProvider16(buffer) {
                RightVolume = (float) 0.5,
                LeftVolume = (float) 0.5
            };

            driver.Init(monoBuffer);
        }

        public int GetCurrentLevel(int aChannelId)
        {
            if (aChannelId >= driver.DriverOutputChannelCount) {
                throw new ArgumentException("Номер канала превышает количество каналов: " + aChannelId);
            }

            if (aChannelId != driver.ChannelOffset) {
                return 0;
            }

            return currentLevel;
        }

        public void SetSamplePerSecond(int aSamplePerSecond)
        {
            samplePerSec = aSamplePerSecond;            
        }

        public void SetBytePerSample(int aBytePerSample)
        {
            bytePerSample = aBytePerSample;
        }

        public int GetChannelsCount()
        {
            return driver.DriverOutputChannelCount;
        }
        
        public void WriteDataToChannel(byte[] aSoundData, int aChannelId)
        {
            var stepSpeed = 100; // миллисекунды
            var bytePerStep = bytePerSample * samplePerSec * stepSpeed / 1000;

            lock (locker) {                
                buffer.ClearBuffer();
                buffer.AddSamples(aSoundData, 0, aSoundData.Length);
                driver.ChannelOffset = aChannelId;          
                driver.Play();
                
                var stepsCount = buffer.BufferedDuration.TotalMilliseconds / stepSpeed;
                for (var stepNumber = 0; stepNumber < stepsCount; ++stepNumber) {
                    var dataIndex = stepNumber * bytePerStep;
                    if (dataIndex < aSoundData.Length) {
                        currentLevel = aSoundData[dataIndex] << 5;                        
                    }

                    Thread.Sleep(stepSpeed);
                }         
       
                driver.Stop();
                currentLevel = 0;
            }
        }

        public void Dispose()
        {
            if (driver != null) {
                driver.Dispose();
            }
        }
    }
}
