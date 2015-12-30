using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Alvasoft.AudioServer.ChannelsManager;
using Alvasoft.AudioServer.Communication;
using Alvasoft.AudioServer.Configuration;
using Alvasoft.AudioServer.SoundsStorage;
using Alvasoft.AudioServer.SoundsStorage.Impl;
using Alvasoft.AudioServer.TimesController;
using Alvasoft.Utils.Activity;
using log4net;

namespace Alvasoft.AudioServer
{
    /// <summary>
    /// Аудиосервер.
    /// </summary>
    public class AudioServer : InitializableImpl, 
                               ControlSessionCallback, 
                               CommandSessionCallback,
                               ServerListenerCallback,
                               TimeControllerCallback
    {
        private static readonly ILog Logger = LogManager.GetLogger("AudioServer");

        private ServerConfiguration configuration;
        private string configurationFileName;

        private ControlListener controlListener;
        private CommandListener commandListener;

        private IChannelManager channelManager;
        private SoundStorage soundStorage;

        private TimeController timeController;        

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public AudioServer()
        {            
            // Имя файла конфигурации.
            var appPath = Application.StartupPath + "\\";
            configurationFileName = appPath + "Configurations\\AudioServerConfiguration.xml";

            // Создаем конфигурацию сервера.
            configuration = new ServerConfiguration();

            controlListener = new ControlListener();
            commandListener = new CommandListener();
        }

        /// <summary>
        /// Возвращает имя файла конфигурации.
        /// </summary>
        /// <returns>Имя файла конфигурации.</returns>
        public string GetConfigurationFileName()
        {
            return configurationFileName;
        }

        /// <summary>
        /// Изменяет имя файла конфигурации.
        /// </summary>
        /// <param name="aFileName">Имя файла конфигурации.</param>
        public void SetConfigurationFileName(string aFileName)
        {
            configurationFileName = aFileName;
        }

        /// <inheritdoc />
        protected override void DoInitialize()
        {
            try {
                Logger.Info("Инициализация...");

                Logger.Info("Имя файла конфигурации \"" + configurationFileName + "\"");
                Logger.Info("Загрузка конфигурации сервера...");
                configuration.LoadFromFile(configurationFileName);

                Logger.Info("Сетевой логин для клиентов: " + configuration.Login);
                Logger.Info("Обнаружено " + configuration.GetOutputDevicesCount() + " устройств вывода.");
                for (int i = 0; i < configuration.GetOutputDevicesCount(); i++) {
                    OutputDeviceInfo outputDevice = configuration.GetOutputDevice(i);
                    Logger.Info("Устройство \"" + outputDevice.GetName() + "\"");
                    Logger.Info("  Тип = " + outputDevice.GetDeviceType());
                    Logger.Info("  Идентификатор = " + outputDevice.GetId());
                }
                Logger.Info("Обнаружено " + configuration.GetOutChannelsCount() + " каналов.");
                for (int j = 0; j < configuration.GetOutChannelsCount(); j++) {
                    OutputChannelInfo channel = configuration.GetOutChannel(j);
                    Logger.Info("Канал \"" + channel.GetName() + "\"");
                    Logger.Info("  Идентификатор = " + channel.GetId());
                    Logger.Info("  Идентификатор устройства = " + channel.GetDevice().GetId());
                    Logger.Info("  Номер канала = " + channel.GetChannelNumber());
                    Logger.Info("  Идентификатор группы = " + channel.GetGroup().GetId());
                }
                Logger.Info("Обнаружено " + configuration.GetChannelGroupsCount() + " групп каналов.");
                Logger.Info("Обнаружено " + configuration.GetInputDevicesCount() + " устройств ввода.");
                for (var i = 0; i < configuration.GetInputDevicesCount(); ++i) {
                    var device = configuration.GetInputDevice(i);
                    Logger.Info("Устройство \"" + device.GetName() + "\"");
                    Logger.Info("  Идентификатор = " + device.GetId());
                }
                Logger.Info("Обнаружено " + configuration.GetInChannelsCount() + " каналов");
                for (var i = 0; i < configuration.GetInChannelsCount(); ++i) {
                    var channel = configuration.GetInChannel(i);
                    Logger.Info("Канал \"" + channel.GetName() + "\"");
                    Logger.Info("  Идентификатор = " + channel.GetId());
                    Logger.Info("  Идентификатор устройства = " + channel.GetDevice().GetId());
                    Logger.Info("  Номер канала = " + channel.GetChannelNumber());                    
                }

                Logger.Info("Загрузка конфигурации сервера завершена.");

                Logger.Info("Конфигурация менеджера каналов...");
                channelManager = new ChannelManagerImpl(configuration);
                channelManager.StartManage();
                Logger.Info("Конфигурация менеджера каналов завершена.");

                Logger.Info("Конфигурация генератора голоса...");
                var generatorConfig = configuration.voiceConfiguration;
                var generator = new VoiceSynthesizer(generatorConfig.Voice, 
                                                        generatorConfig.Rate, 
                                                        generatorConfig.SamplePerSecond);
                Logger.Info("Конфигурация генератора голоса завершена.");

                Logger.Info("Конфигурация источников голосовых сообщений...");
                var providers = new List<SoundProvider> {
                    new WaveFileSoundProvider(configuration.WavFilesFolder),
                    new MapSoundProvider(generator, configuration.MapSoundsFile),
                    new GeneratorSoundProvider(generator)
                };
                soundStorage = new SoundStorage(providers);
                Logger.Info("Конфигурация источников голосовых сообщений завершена.");

                Logger.Info("Конфигурация объявлений времени...");
                timeController = new TimeController();
                timeController.SetCallback(this);
                timeController.LoadFromXmlFile(configuration.TimeControllerFile);
                timeController.Initialize();
                Logger.Info("Конфигурация объявлений времени завершена.");

                controlListener.SetCallback(this);
                controlListener.GetListenEndPoint().Address = IPAddress.Any;
                controlListener.GetListenEndPoint().Port = 9998;

                commandListener.SetCallback(this);
                commandListener.GetListenEndPoint().Address = IPAddress.Any;
                commandListener.GetListenEndPoint().Port = 9999;            

                controlListener.Initialize();
                commandListener.Initialize();

                Logger.Info("Инициализация завершена.");
            }
            catch (Exception ex) {
                Logger.Info("Загрузка конфигурации не выполнена: " + ex.Message);
                Process.GetCurrentProcess().Kill();
            }
        }

        /// <inheritdoc />
        protected override void DoUninitialize()
        {
            Logger.Info("Деинициализация AudioServer...");
            timeController.Uninitialize();
            commandListener.Uninitialize();
            controlListener.Uninitialize();
            channelManager.StopManage();
            Logger.Info("Деинициализация AudioServer завершена.");
        }

        /// <summary>
        /// Реагирует при потерисоединения с клиентом.
        /// </summary>
        /// <param name="aControlSession">ControlSession, для которой потеряна связь.</param>
        public void OnCloseConnection(ControlSession aControlSession)
        {
            controlListener.OnCloseConnection(aControlSession);
        }

        /// <summary>
        /// Реагирует при запросе количества управляющий соединений.
        /// </summary>
        /// <param name="aControlSession">Сессия клиента, выполнившего запрос.</param>
        /// <param name="aSessionCount">Выходной параметр - количество управляющий сессий.</param>
        public void OnQueryStatus(ControlSession aControlSession, out int aSessionCount)
        {
            aSessionCount = controlListener.GetSessionsCount();
        }

        /// <summary>
        /// Реагирует при запросе клиентом информации о каналах.
        /// </summary>
        /// <param name="aControlSession">Сессия клиенты, выполнившего запрос.</param>
        /// <param name="aChannelIds">Выходной параметр - идентификаторы каналов.</param>
        /// <param name="aChannelNames">Выходной параметр - заголовки каналов.</param>
        public void OnChannelInfo(ControlSession aControlSession, out int[] aChannelIds, out string[] aChannelNames)
        {
            var channelsCount = configuration.GetOutChannelsCount();
            aChannelIds = new int[channelsCount];
            aChannelNames = new string[channelsCount];

            for (var i = 0; i < channelsCount; ++i) {
                var channel = configuration.GetOutChannel(i);
                aChannelIds[i] = (int) channel.GetId();
                aChannelNames[i] = channel.GetName();
            }
        }

        /// <summary>
        /// Реагирует при запросе клиентом входных каналов.
        /// </summary>
        /// <param name="aControlSession">Сесссия клиента, выполнившего запрос.</param>
        /// <param name="aInputLineIds">Выходной параметр - идентификаторы входных каналов.</param>
        /// <param name="aInputLineNames">Выходной параметр - заголовки входных каналов.</param>
        public void OnInputLine(ControlSession aControlSession, out int[] aInputLineIds, out string[] aInputLineNames)
        {
            var inputLinesCount = configuration.GetInChannelsCount();
            aInputLineIds = new int[inputLinesCount];
            aInputLineNames = new string[inputLinesCount];
            for (var i = 0; i < inputLinesCount; ++i) {
                var inputLine = configuration.GetInChannel(i);
                aInputLineIds[i] = (int) inputLine.GetId();
                aInputLineNames[i] = inputLine.GetName();
            }
        }

        /// <summary>
        /// Реагирует при запросе клиентом уровня звука на выходных каналах.
        /// </summary>
        /// <param name="aControlSession">Сессия клиенты, выполнившего запрос.</param>
        /// <param name="aChannelIds">Идентификаторы каналов.</param>
        /// <param name="aChannelLevels">Уровни каналов.</param>
        public void OnChannelData(ControlSession aControlSession, out int[] aChannelIds, out int[] aChannelLevels)
        {
            var channelsCount = configuration.GetOutChannelsCount();

            aChannelIds = new int[channelsCount];
            aChannelLevels = new int[channelsCount];

            for (var i = 0; i < channelsCount; ++i) {
                var channel = configuration.GetOutChannel(i);     
                var channelId = (int) channel.GetId();
                aChannelIds[i] = channelId;
                aChannelLevels[i] = channelManager.GetChannelById(channelId).GetCurrentLevel();
            }
        }

        /// <summary>
        /// Реагирует при получении от клиента команды на выключение аудиосервера.
        /// </summary>
        /// <param name="aControlSession">Сессия клиента, пославшего команды выключения.</param>
        public void OnShutdown(ControlSession aControlSession)
        {            
            Uninitialize();
            Logger.Info("Сервер был остановлен с консоли.");
            Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// Реагирует при получении от клиента команды на запись звукового сообщения.
        /// </summary>
        /// <param name="aControlSession">Сессия клиента, выполнившего команду.</param>
        /// <param name="aInputLineId">Идентификатор входного канала.</param>
        /// <param name="aChannelsIds">Идентификаторы каналов для вывода сообщения.</param>
        public void OnStartRecordSound(ControlSession aControlSession, int aInputLineId, int[] aChannelsIds)
        {
            channelManager.StartRecord(aInputLineId, aChannelsIds, aControlSession.GetHashCode());
        }

        /// <summary>
        /// Реагирует при получении от клиента команды на прикращение записи голосовгосообщения.
        /// </summary>
        /// <param name="aControlSession">Сессия клиента, выполнившегокоманду.</param>
        public void OnStopRecordSound(ControlSession aControlSession)
        {
            channelManager.StopRecord(aControlSession.GetHashCode());
        }

        /// <summary>
        /// Реагирует при получении от клиента команды на воспроизменение звукового сообщения.
        /// </summary>
        /// <param name="aCommandSession">Сессия клиента, выполнившего команду.</param>
        /// <param name="aChannelIds">Идентификаторы выходных каналов для звукового сообщения.</param>
        /// <param name="aPriotity">Приоритет сообщения.</param>
        /// <param name="aFiles">Файлы для воспроизведения.</param>
        public void OnPlaySound(CommandSession aCommandSession, int[] aChannelIds, int aPriotity, string[] aFiles)
        {            
            var data = soundStorage.ProvideSound(aFiles);
            var message = new SoundMessage(data, (uint) aPriotity);
            foreach (var channelId in aChannelIds) {
                channelManager.ProcessMessage(message, channelId);
            }
        }

        /// <summary>
        /// Реагирует при создании нового подключения.
        /// </summary>
        /// <param name="aServerListener">Слушатель.</param>
        /// <param name="aClientSession">Созданная сессия.</param>
        public void OnCreatedSession(AbstractServerListener aServerListener, AbstractClientSession aClientSession)
        {            
            var commandSession = aClientSession as CommandSession;
            if (commandSession != null) {
                commandSession.SetCallback(this);
                return;
            }

            var controlSession = aClientSession as ControlSession;
            if (controlSession != null) {
                controlSession.SetCallback(this);
                controlSession.SetLogin(configuration.Login);
                return;
            }

            throw new ArgumentException("WTF?");
        }

        /// <summary>
        /// Реагирует при закрытии соединения с клиентом.
        /// </summary>
        /// <param name="aServerListener">Слушатель.</param>
        /// <param name="aClientSession">Сессия клиента, соединение с которым прервано.</param>
        public void OnCloseConnection(AbstractServerListener aServerListener, AbstractClientSession aClientSession)
        {
            Logger.Info("Соединение закрыто.");
        }

        /// <summary>
        /// Реагирует при получении команды от контроллера времени на объявление сообщения времени.
        /// </summary>
        /// <param name="aChannelIds">Идентификаторы выходных каналов для объявления сообщения времени.</param>
        /// <param name="aPriority">Приоритет сообщения.</param>
        /// /// <param name="aPrefixSound">Префикс для объявлния времени.</param>
        /// <param name="aTimePhrase">Фраза для произношения.</param>
        public void OnTimeAnnounce(int[] aChannelIds, int aPriority, byte[] aPrefixSound, string aTimePhrase)
        {
            var timeSound = soundStorage.ProvideSound(new[] { aTimePhrase });
            var data = timeSound;
            if (aPrefixSound != null) {
                data = Enumerable.Concat(aPrefixSound, timeSound).ToArray();
            }
            var message = new SoundMessage(data, (uint) aPriority);
            foreach (var channelId in aChannelIds) {
                channelManager.ProcessMessage(message, channelId);
            }
        }
    }
}