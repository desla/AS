using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NAudio.Wave.Asio;
using System.Threading;

namespace NAudio.Wave
{
    /// <summary>
    /// Расширение AsioOut, которое позволяет работать с каналами ввода/вывода параллельно.
    /// Денис Зинченко, ООО АльваСофт, 2015
    /// denis.zinchenko@alvasoft.ru
    /// </summary>
    public class AsioOutExtended : IWavePlayer
    {
        private ASIODriverExtExtended driver;
        private WaveFormat sourceFormat;
        private PlaybackState playbackState;
        private int nbSamples;        
        private ASIOSampleConvertor.SampleConvertor convertor;
        private readonly string driverName;        

        private readonly SynchronizationContext syncContext;

        public IWaveProvider[] Sources;
        private List<byte[]> sourcesBuffers; 

        /// <summary>
        /// Playback Stopped
        /// </summary>
        public event EventHandler<StoppedEventArgs> PlaybackStopped;

        /// <summary>
        /// When recording, fires whenever recorded audio is available
        /// </summary>
        public event EventHandler<AsioAudioAvailableEventArgs> AudioAvailable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsioOut"/> class with the first 
        /// available ASIO Driver.
        /// </summary>
        public AsioOutExtended()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsioOut"/> class with the driver name.
        /// </summary>
        /// <param name="driverName">Name of the device.</param>
        public AsioOutExtended(String driverName)
        {
            this.syncContext = SynchronizationContext.Current;            
            InitFromName(driverName);
        }

        /// <summary>
        /// Opens an ASIO output device
        /// </summary>
        /// <param name="driverIndex">Device number (zero based)</param>
        public AsioOutExtended(int driverIndex)
        {
            this.syncContext = SynchronizationContext.Current; 
            String[] names = GetDriverNames();
            if (names.Length == 0)
            {
                throw new ArgumentException("There is no ASIO Driver installed on your system");
            }
            if (driverIndex < 0 || driverIndex > names.Length)
            {
                throw new ArgumentException(String.Format("Invalid device number. Must be in the range [0,{0}]", names.Length));
            }
            this.driverName = names[driverIndex];
            InitFromName(this.driverName);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="AsioOut"/> is reclaimed by garbage collection.
        /// </summary>
        ~AsioOutExtended()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (driver != null)
            {
                if (playbackState != PlaybackState.Stopped)
                {
                    driver.Stop();
                }
                driver.ReleaseDriver();
                driver = null;
            }
        }

        /// <summary>
        /// Gets the names of the installed ASIO Driver.
        /// </summary>
        /// <returns>an array of driver names</returns>
        public static String[] GetDriverNames()
        {
            return ASIODriver.GetASIODriverNames();
        }

        /// <summary>
        /// Determines whether ASIO is supported.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if ASIO is supported; otherwise, <c>false</c>.
        /// </returns>
        public static bool isSupported()
        {
            return GetDriverNames().Length > 0;
        }

        /// <summary>
        /// Inits the driver from the asio driver name.
        /// </summary>
        /// <param name="driverName">Name of the driver.</param>
        private void InitFromName(String driverName)
        {
            // Get the basic driver
            ASIODriver basicDriver = ASIODriver.GetASIODriverByName(driverName);            

            // Instantiate the extended driver
            driver = new ASIODriverExtExtended(basicDriver);            
        }

        /// <summary>
        /// Shows the control panel
        /// </summary>
        public void ShowControlPanel()
        {
            driver.ShowControlPanel();
        }

        /// <summary>
        /// Starts playback
        /// </summary>
        public void Play()
        {
            if (playbackState != PlaybackState.Playing)
            {
                playbackState = PlaybackState.Playing;
                driver.Start();
            }
        }

        /// <summary>
        /// Stops playback
        /// </summary>
        public void Stop()
        {
            playbackState = PlaybackState.Stopped;
            driver.Stop();
            RaisePlaybackStopped(null);
        }

        /// <summary>
        /// Pauses playback
        /// </summary>
        public void Pause()
        {
            playbackState = PlaybackState.Paused;
            driver.Stop();
        }

        public void Init(IWaveProvider waveProvider)
        {
            Init(waveProvider.WaveFormat);
        }

        /// <summary>
        /// Initialises to play
        /// </summary>
        /// <param name="waveProvider">Source wave provider</param>
        public void Init(WaveFormat waveFormat)
        {
            InitRecordAndPlayback(waveFormat, 0, -1);
        }

        /// <summary>
        /// Initialises to play, with optional recording
        /// </summary>
        /// <param name="waveProvider">Source wave provider - set to null for record only</param>
        /// <param name="recordChannels">Number of channels to record</param>
        /// <param name="recordOnlySampleRate">Specify sample rate here if only recording, ignored otherwise</param>
        public void InitRecordAndPlayback(WaveFormat waveFormat, int recordChannels, int recordOnlySampleRate)
        {
            if (sourceFormat != null)
            {
                throw new InvalidOperationException("Already initialised this instance of AsioOut - dispose and create a new one");
            }
            int desiredSampleRate = waveFormat != null ? waveFormat.SampleRate : recordOnlySampleRate;

            if (waveFormat != null)
            {
                NumberOfOutputChannels = waveFormat.Channels;

                // Select the correct sample convertor from WaveFormat -> ASIOFormat
                convertor = ASIOSampleConvertor.SelectSampleConvertor(waveFormat, driver.Capabilities.OutputChannelInfos[0].type);
            }
            else
            {
                NumberOfOutputChannels = 0;
            }

            if (!driver.IsSampleRateSupported(desiredSampleRate))
            {
                throw new ArgumentException("SampleRate is not supported");
            }

            if (driver.Capabilities.SampleRate != desiredSampleRate)
            {
                driver.SetSampleRate(desiredSampleRate);
            }

            // Plug the callback
            driver.FillBufferCallback = driver_BufferUpdate;

            NumberOfInputChannels = recordChannels;
            // Used Prefered size of ASIO Buffer
            nbSamples = driver.CreateBuffers(NumberOfOutputChannels, NumberOfInputChannels, false);

            if (waveFormat != null) {
                var bufferLength = nbSamples * NumberOfOutputChannels * waveFormat.BitsPerSample / 8;
                sourcesBuffers = new List<byte[]>();
                for (var i = 0; i < driver.Capabilities.NbOutputChannels; ++i) {
                    sourcesBuffers.Add(new byte[bufferLength]);
                }
            }            
        }

        /// <summary>
        /// driver buffer update callback to fill the wave buffer.
        /// </summary>
        /// <param name="inputChannels">The input channels.</param>
        /// <param name="outputChannels">The output channels.</param>
        void driver_BufferUpdate(IntPtr[] inputChannels, IntPtr[] outputChannels)
        {            
            if (this.NumberOfInputChannels > 0)
            {
                var audioAvailable = AudioAvailable;
                if (audioAvailable != null)
                {
                    var args = new AsioAudioAvailableEventArgs(inputChannels, outputChannels, nbSamples,
                                                               driver.Capabilities.InputChannelInfos[0].type);
                    audioAvailable(this, args);
                    if (args.WrittenToOutputBuffers)
                        return;
                }
            }            

            if (this.NumberOfOutputChannels > 0) {                
                var bufferLength = sourcesBuffers[0].Length;
                if (Sources != null) {                    
                    for (var i = 0; i < Sources.Length; ++i) {
                        if (Sources[i] != null) {
                            var source = Sources[i];                            
                            source.Read(sourcesBuffers[i], 0, bufferLength);
                            var currentChannels = new IntPtr[NumberOfOutputChannels];
                            for (var j = 0; j < NumberOfOutputChannels; ++j) {
                                currentChannels[j] = outputChannels[i + j];
                            }
                            unsafe {
                                fixed (void* pBuffer = &(sourcesBuffers[i][0])) {
                                    convertor(new IntPtr(pBuffer), currentChannels, NumberOfOutputChannels, nbSamples);
                                }
                            }
                        } // if
                    } // for
                } // if
            } // if
        }

        /// <summary>
        /// Gets the latency (in ms) of the playback driver
        /// </summary>
        public int PlaybackLatency
        {
            get
            {
                int latency, temp;
                driver.Driver.GetLatencies(out temp, out latency);
                return latency;
            }
        }

        /// <summary>
        /// Playback State
        /// </summary>
        public PlaybackState PlaybackState
        {
            get { return playbackState; }
        }

        /// <summary>
        /// Driver Name
        /// </summary>
        public string DriverName
        {
            get { return this.driverName; }
        }

        /// <summary>
        /// The number of output channels we are currently using for playback
        /// (Must be less than or equal to DriverOutputChannelCount)
        /// </summary>
        public int NumberOfOutputChannels { get; private set; }

        /// <summary>
        /// The number of input channels we are currently recording from
        /// (Must be less than or equal to DriverInputChannelCount)
        /// </summary>
        public int NumberOfInputChannels { get; private set; }

        /// <summary>
        /// The maximum number of input channels this ASIO driver supports
        /// </summary>
        public int DriverInputChannelCount { get { return driver.Capabilities.NbInputChannels; } }
        
        /// <summary>
        /// The maximum number of output channels this ASIO driver supports
        /// </summary>
        public int DriverOutputChannelCount { get { return driver.Capabilities.NbOutputChannels; } }

        /// <summary>
        /// Sets the volume (1.0 is unity gain)
        /// Not supported for ASIO Out. Set the volume on the input stream instead
        /// </summary>
        [Obsolete("this function will be removed in a future NAudio as ASIO does not support setting the volume on the device")]
        public float Volume
        {
            get
            {
                return 1.0f;
            }
            set
            {
                if (value != 1.0f)
                {
                    throw new InvalidOperationException("AsioOut does not support setting the device volume");
                }
            }
        }

        private void RaisePlaybackStopped(Exception e)
        {
            var handler = PlaybackStopped;
            if (handler != null)
            {
                if (this.syncContext == null)
                {
                    handler(this, new StoppedEventArgs(e));
                }
                else
                {
                    this.syncContext.Post(state => handler(this, new StoppedEventArgs(e)), null);
                }
            }
        }

        /// <summary>
        /// Get the input channel name
        /// </summary>
        /// <param name="channel">channel index (zero based)</param>
        /// <returns>channel name</returns>
        public string AsioInputChannelName(int channel)
        {
            return channel > DriverInputChannelCount ? "" : driver.Capabilities.InputChannelInfos[channel].name;
        }

        /// <summary>
        /// Get the output channel name
        /// </summary>
        /// <param name="channel">channel index (zero based)</param>
        /// <returns>channel name</returns>
        public string AsioOutputChannelName(int channel)
        {
            return channel > DriverOutputChannelCount ? "" : driver.Capabilities.OutputChannelInfos[channel].name;
        }
    }
}
