using System;
using System.Threading;
using NAudio.Wave;

namespace Alvasoft.AudioServer.ChannelsManager.Impl.Devices
{
    /// <summary>
    /// Устройство ввода с поддержкой протокола Asio.
    /// </summary>
    public class AsioInputDevice : IInputDevice
    {
        private const int MAX_RECORD_SECONDS = 2 * 60;
        private const int SAMPLE_RATE = 44100;
        private IChannelManager callback;
        private AsioOutExtended asioDriver;        
        private AsioDeviceChannel[] channels;
        private MonoToStereoProvider16[] converters;
        private int[][] outputChannels;        
        private Thread[] recordControlThreads;        

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aDriverName">Имя asio-драйвера.</param>
        public AsioInputDevice(string aDriverName)
        {            
            asioDriver = new AsioOutExtended(aDriverName);
            var channelsCount = asioDriver.DriverInputChannelCount;
            channels = new AsioDeviceChannel[channelsCount];            
            for (var i = 0; i < channelsCount; ++i) {
                channels[i] = new AsioDeviceChannel {
                    Locker = new object(),
                    RecordOwner = -1,
                    IsBusy = false,
                    Buffer = new BufferedWaveProvider(new WaveFormat(SAMPLE_RATE, 1))
                };                
            }
            converters = new MonoToStereoProvider16[channelsCount];
            for (var i = 0; i < channelsCount; ++i) {
                converters[i] = new MonoToStereoProvider16(channels[i].Buffer) {
                    LeftVolume = 1,
                    RightVolume = 1
                };
            }
            outputChannels = new int[channelsCount][];
            recordControlThreads = new Thread[channelsCount];

            asioDriver.AudioAvailable += AudioAvailable;
            asioDriver.InitRecordAndPlayback(null, 1, SAMPLE_RATE);
            asioDriver.Play();
        }        

        /// <summary>
        /// Возвращает количество физических каналов на устройстве.
        /// </summary>
        /// <returns></returns>
        public int GetChannelsCount()
        {
            return asioDriver.DriverInputChannelCount;
        }

        /// <summary>
        /// Начинает запись на указанном канале.
        /// </summary>
        /// <param name="aInChannelNumber">Номер канала для записи.</param>
        /// <param name="aOutChannelsIds">Выходные каналы.</param>
        /// <param name="aRecordOwner">Инициатор записи.</param>
        public void StartRecord(int aInChannelNumber, int[] aOutChannelsIds, int aRecordOwner)
        {
            var channel = channels[aInChannelNumber];
            lock (channel.Locker) {
                if (channel.IsBusy) {
                    return;
                }
                channel.IsBusy = true;
                channel.RecordOwner = aRecordOwner;
                outputChannels[aInChannelNumber] = aOutChannelsIds;                
                recordControlThreads[aInChannelNumber] = 
                    new Thread(() => RecordControlMethod(aInChannelNumber, aRecordOwner));
                recordControlThreads[aInChannelNumber].Start();                
            }
        }

        /// <summary>
        /// Прекращает запись на канале.
        /// </summary>
        /// <param name="aChannelNumber">Номер канала.</param>
        /// <param name="aRecordOwner">Инициатор записи.</param>
        public void StopRecord(int aChannelNumber, int aRecordOwner)
        {
            var channel = channels[aChannelNumber];
            lock (channel.Locker) {
                if (!channel.IsBusy || channel.RecordOwner != aRecordOwner) {
                    return;
                }

                channel.IsBusy = false;
                channel.RecordOwner = -1;                

                if (Thread.CurrentThread != recordControlThreads[aChannelNumber]) {
                    recordControlThreads[aChannelNumber].Abort();
                }

                var bufferedBytes = channel.Buffer.BufferedBytes<<1;
                var sound = new byte[bufferedBytes];                  
                converters[aChannelNumber].Read(sound, 0, bufferedBytes);                
                channel.Buffer.ClearBuffer();

                if (callback != null) {
                    if (sound.Length > 0) {
                        var message = new SoundMessage(sound, 255);
                        foreach (var outputChannel in outputChannels[aChannelNumber]) {
                            callback.ProcessMessage(message, outputChannel);
                        }
                    }                    
                }                
            }
        }

        /// <summary>
        /// Устанавливает callback для отправки звукового сообщения на канал.
        /// </summary>
        /// <param name="aCallback">Callback.</param>
        public void SetCallback(IChannelManager aCallback)
        {
            callback = aCallback;
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

        /// <summary>
        /// Асинхронно срабатывает при заполнении физических буферов устройства.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void AudioAvailable(object sender, AsioAudioAvailableEventArgs e)
        {
            for (var i = 0; i < channels.Length; ++i) {                
                if (channels[i].IsBusy) {
                    var samples = new float[e.SamplesPerBuffer];
                    var soundLength = e.GetAsInterleavedSamplesExtended(samples, i);
                    RecordSoundOnChannel(channels[i], samples, soundLength);
                }
            }            
            e.WrittenToOutputBuffers = true;
        }

        /// <summary>
        /// Сохраняет переданный звук в буфере канала.
        /// </summary>
        /// <param name="aChannel">Канал записи.</param>
        /// <param name="aBuffer">Беферизированный звук.</param>
        /// <param name="aBufferSize">Размер буфера звука.</param>
        private void RecordSoundOnChannel(AsioDeviceChannel aChannel, float[] aBuffer, int aBufferSize)
        {
            var byteBuffer = new byte[aBufferSize<<1];
            for (var i = 0; i < aBufferSize; ++i) {
                var bytes = BitConverter.GetBytes((Int16)(Int16.MaxValue *  aBuffer[i]));
                var index = i << 1;
                byteBuffer[index] = bytes[0];
                byteBuffer[index + 1] = bytes[1];
            }
            aChannel.Buffer.AddSamples(byteBuffer, 0, byteBuffer.Length);
        }


        /// <summary>
        /// Контролирует продолжительность записи сообщения.
        /// Прекращает запись по истечении указанного времени.
        /// </summary>
        /// <param name="aInChannelId">Номер канала, для которого идет запись.</param>
        /// <param name="aRecordOwner">Идентификатор инициатора записи.</param>
        private void RecordControlMethod(int aInChannelId, int aRecordOwner)
        {
            Thread.Sleep(MAX_RECORD_SECONDS * 1000);
            StopRecord(aInChannelId, aRecordOwner);
        }
    }
}
