using System;
using System.Threading;
using Alvasoft.AudioServer.ChannelsManager;
using NAudio.Wave;

namespace AudioServer.Alvasoft.AudioServer.ChannelsManager.Impl.Devices
{
    public class AsioInputDevice : IInputDevice
    {
        private int sampleRate = 44100;
        private IChannelManager callback;
        private AsioOut driver;
        private WaveFormat format;
        private BufferedWaveProvider buffer;
        private MonoToStereoProvider16 converter;
        private object recordLock = new object();
        private bool isBusy = false;
        private int[] outputChannels;
        private int recordOwner = -1;

        private Thread recordControlThread;
        private int maxRecordSeconds = 2 * 60;

        public AsioInputDevice(string aDriverName)
        {
            driver = new AsioOut(aDriverName);
            format = new WaveFormat(sampleRate, 1);
            buffer = new BufferedWaveProvider(format);
            converter = new MonoToStereoProvider16(buffer) {
                LeftVolume = (float) 0.5,
                RightVolume = (float) 0.5
            };            
            driver.AudioAvailable += AudioAvailable;
            driver.InitRecordAndPlayback(null, 1, sampleRate);
        }        

        public int GetChannelsCount()
        {
            return driver.DriverInputChannelCount;
        }

        public void StartRecord(int aInChannelNumber, int[] aOutChannelsIds, int aRecordOwner)
        {
            lock (recordLock) {
                if (isBusy) {
                    return;
                }
                isBusy = true;
                driver.InputChannelOffset = aInChannelNumber;
                outputChannels = aOutChannelsIds;
                recordOwner = aRecordOwner;
                recordControlThread = new Thread(() => RecordControlMethod(aInChannelNumber, aRecordOwner));
                recordControlThread.Start();
                driver.Play();
            }
        }

        public void StopRecord(int aChannelNumber, int aRecordOwner)
        {
            lock (recordLock) {
                if (!isBusy || aRecordOwner != recordOwner) {
                    return;
                }
                driver.Stop();
                recordOwner = -1;
                isBusy = false;

                if (Thread.CurrentThread != recordControlThread) {
                    recordControlThread.Abort();
                }

                var bufferedBytes = buffer.BufferedBytes << 1;
                var sound = new byte[bufferedBytes];                  
                converter.Read(sound, 0, bufferedBytes);
                buffer.ClearBuffer();

                if (callback != null) {
                    var message = new SoundMessage(sound, 255);
                    foreach (var channel in outputChannels) {
                        callback.ProcessMessage(message, channel);
                    }
                }                
            }
        }

        public void SetCallback(IChannelManager aCallback)
        {
            callback = aCallback;
        }

        public void Dispose()
        {
            if (driver != null) {
                driver.Dispose();
            }
        }

        private void AudioAvailable(object sender, AsioAudioAvailableEventArgs e)
        {
            var floatBuffer = e.GetAsInterleavedSamples();
            var byteBuffer = new byte[floatBuffer.Length << 1];
            for (var i = 0; i < floatBuffer.Length; ++i) {
                var bytes = BitConverter.GetBytes((Int16)(Int16.MaxValue * floatBuffer[i]));
                var index = i << 1;
                byteBuffer[index] = bytes[0];
                byteBuffer[index + 1] = bytes[1];
            }
            buffer.AddSamples(byteBuffer, 0, byteBuffer.Length);
        }

        /// <summary>
        /// Контролирует продолжительность записи сообщения.
        /// Прекращает запись по истечении указанного времени.
        /// </summary>
        /// <param name="aInChannelId"></param>
        /// <param name="aRecordOwner"></param>
        private void RecordControlMethod(int aInChannelId, int aRecordOwner)
        {
            Thread.Sleep(maxRecordSeconds * 1000);
            StopRecord(aInChannelId, aRecordOwner);
        }
    }
}
