using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Alvasoft.Utils.Common;
using log4net;
using log4net.Repository.Hierarchy;

namespace Alvasoft.AudioServer.Configuration
{
    /// <summary>
    /// Конфигурация сервера.
    /// </summary>
    public class ServerConfiguration
    {
        private static readonly ILog Logger = LogManager.GetLogger("ServerConfiguration");

        private const string NODE_OUTPUT_DEVICES = "outputDevices";
        private const string NODE_INPUT_DEVICES = "inputDevices";
        private const string NODE_DEVICE = "device";
        private const string NODE_TYPE = "type";
        private const string NODE_DEVICE_NAME = "name";
        private const string NODE_CHANNELS = "channels";
        private const string NODE_CHANNEL = "channel";
        private const string NODE_CHANNEL_ID = "id";
        private const string NODE_CHANNEL_NAME = "name";
        private const string NODE_CHANNEL_NUMBER_ON_DEVICE = "deviceChannel";
        private const string NODE_CHANNEL_GROUP = "group";
        private const string NODE_MAP_SOUND = "mapSoundsConfiguration";
        private const string NODE_TIME_CONTROLLER = "timeControllerConfiguration";
        private const string NODE_VOICE_GENERATOR = "voiceGenerator";
        private const string NODE_VOICE = "voice";
        private const string NODE_RATE = "rate";
        private const string NODE_SAMPLE_PER_SECOND = "samplePerSecond";
        private const string NODE_WAV_FILES_FOLDER = "wavFilesFolder";
        private const string NODE_LOGIN = "login";

        public string MapSoundsFile { get; private set; }
        public string TimeControllerFile { get; private set; }
        public string WavFilesFolder { get; private set; }
        public string Login { get; private set; }

        public VoiceGeneratorInfo voiceConfiguration = new VoiceGeneratorInfo();

        private List<OutputDeviceInfoImpl> outputDevices = new List<OutputDeviceInfoImpl>();
        private List<OutputChannelInfoImpl> outputChannels = new List<OutputChannelInfoImpl>();
        private List<ChannelGroupInfoImpl> channelGroups = new List<ChannelGroupInfoImpl>();

        private List<InputDeviceInfoImpl> inputDevices = new List<InputDeviceInfoImpl>();
        private List<InputChannelInfoImpl> inputChannels = new List<InputChannelInfoImpl>();

        /// <summary>
        /// Загружает конфигурацию из файла.
        /// </summary>
        /// <param name="aFileName">Имя файла конфигурации.</param>
        public void LoadFromFile(string aFileName)
        {
            if (string.IsNullOrEmpty(aFileName)) {
                throw new ArgumentNullException("aFileName");
            }

            var document = new XmlDocument();
            document.Load(aFileName);

            var root = document.DocumentElement;

            if (root == null) {
                throw new XmlException(aFileName + " is not contain root element");
            }

            var items = root.ChildNodes;

            for (var itemIndex = 0; itemIndex < items.Count; ++itemIndex) {
                switch (items[itemIndex].Name) {
                    case NODE_LOGIN:
                        Login = items[itemIndex].InnerText;
                        break;
                    case NODE_OUTPUT_DEVICES:
                        var outDevices = items[itemIndex].ChildNodes;
                        for (var deviceIndex = 0; deviceIndex < outDevices.Count; ++deviceIndex) {
                            LoadOutputDeviceFromXml(outDevices[deviceIndex]);
                        }
                        break;
                    case NODE_INPUT_DEVICES:
                        var inputDevices = items[itemIndex].ChildNodes;
                        for (var deviceIndex = 0; deviceIndex < inputDevices.Count; ++deviceIndex) {
                            LoadInputDeviceFromXml(inputDevices[deviceIndex]);
                        }
                        break;
                    case NODE_TIME_CONTROLLER:
                        TimeControllerFile = items[itemIndex].InnerText;                        
                        break;
                    case NODE_MAP_SOUND:
                        MapSoundsFile = items[itemIndex].InnerText;                        
                        break;
                    case NODE_WAV_FILES_FOLDER:
                        WavFilesFolder = items[itemIndex].InnerText;
                        break;
                    case NODE_VOICE_GENERATOR:
                        LoadVoiceConfigureFromXml(items[itemIndex]);
                        break;                    
                }                
            }
            
            // Подготавливаем загруженную конфигурацию к использованию.
            PrepareConfiguration();
        }

        /// <summary>
        /// Загружает конфигурацию для устройств ввода.
        /// </summary>
        /// <param name="aInputDevice">XmlNode с настройками.</param>
        private void LoadInputDeviceFromXml(XmlNode aInputDevice)
        {
            var deviceInfo = new InputDeviceInfoImpl();
            deviceInfo.SetId(inputDevices.Count);
            for (var fieldIndex = 0; fieldIndex < aInputDevice.ChildNodes.Count; ++fieldIndex) {
                var field = aInputDevice.ChildNodes[fieldIndex];
                switch (field.Name) {
                    case NODE_DEVICE_NAME:
                        deviceInfo.SetName(field.InnerText);
                        break;

                    case NODE_TYPE:
                        deviceInfo.SetDeviceType(field.InnerText);
                        break;

                    case NODE_CHANNELS:
                        for (var channelIndex = 0; channelIndex < field.ChildNodes.Count; ++channelIndex) {
                            LoadInChannelFromXml(field.ChildNodes[channelIndex], deviceInfo.GetId());
                        }
                        break;

                    default:
                        throw new XmlException(string.Format("Unknown item {0} in device configuration", field.Name));
                }
            }

            inputDevices.Add(deviceInfo);
        }

        /// <summary>
        /// Загружает конфигурацию входных каналов.
        /// </summary>
        /// <param name="aChannel">XmlNode с настройками.</param>
        /// <param name="aDeviceId">Идентификатор устройства ввода.</param>
        private void LoadInChannelFromXml(XmlNode aChannel, long aDeviceId)
        {
            var channelInfo = new InputChannelInfoImpl();
            channelInfo.SetDeviceId(aDeviceId);
            for (var fieldIndex = 0; fieldIndex < aChannel.ChildNodes.Count; ++fieldIndex) {
                var field = aChannel.ChildNodes[fieldIndex];
                switch (field.Name) {
                    case NODE_CHANNEL_ID:
                        channelInfo.SetId(Convert.ToInt64(field.InnerText));
                        break;

                    case NODE_CHANNEL_NAME:
                        channelInfo.SetName(field.InnerText);
                        break;

                    case NODE_CHANNEL_NUMBER_ON_DEVICE:
                        channelInfo.SetChannelNumber(Convert.ToInt32(field.InnerText));
                        break;

                    default:
                        throw new XmlException(string.Format("Unknown item {0} in channel configuration", field.Name));
                }
            }

            inputChannels.Add(channelInfo);
        }

        /// <summary>
        /// Загружает конфигурацию генератора голоса.
        /// </summary>
        /// <param name="aNode">XmlNode c настройками.</param>
        private void LoadVoiceConfigureFromXml(XmlNode aNode)
        {
            var items = aNode.ChildNodes;
            for (var itemIndex = 0; itemIndex < items.Count; ++itemIndex) {
                switch (items[itemIndex].Name) {
                    case NODE_VOICE:
                        voiceConfiguration.Voice = items[itemIndex].InnerText;
                        break;
                    case NODE_RATE:
                        voiceConfiguration.Rate = Convert.ToInt32(items[itemIndex].InnerText);
                        break;
                    case NODE_SAMPLE_PER_SECOND:
                        voiceConfiguration.SamplePerSecond = Convert.ToInt32(items[itemIndex].InnerText);
                        break;
                }
            }
        }

        /// <summary>
        /// Загружает конфигурацию выходных устройств.
        /// </summary>
        /// <param name="device">XmlNode с настройками.</param>
        private void LoadOutputDeviceFromXml(XmlNode device)
        {
            var deviceInfo = new OutputDeviceInfoImpl();
            deviceInfo.SetId(outputDevices.Count);
            for (var fieldIndex = 0; fieldIndex < device.ChildNodes.Count; ++fieldIndex) {
                var field = device.ChildNodes[fieldIndex];
                switch (field.Name) {
                    case NODE_TYPE:                        
                        deviceInfo.SetDeviceType(field.InnerText);
                        break;

                    case NODE_DEVICE_NAME:
                        deviceInfo.SetName(field.InnerText);
                        break;

                    case NODE_CHANNELS:
                        for (var channelIndex = 0; channelIndex < field.ChildNodes.Count; ++channelIndex) {
                            LoadOutChannelFromXml(field.ChildNodes[channelIndex], deviceInfo.GetId());
                        }
                        break;

                    default:
                        throw new XmlException(string.Format("Unknown item {0} in device configuration", field.Name));
                }
            }

            outputDevices.Add(deviceInfo);
        }

        /// <summary>
        /// Загружает конфигурацию выходных каналов.
        /// </summary>
        /// <param name="channel">XmlNode с настройками.</param>
        /// <param name="deviceId">Идентификатор устройства вывода.</param>
        private void LoadOutChannelFromXml(XmlNode channel, long deviceId)
        {
            var channelInfo = new OutputChannelInfoImpl();
            channelInfo.SetDeviceId(deviceId);
            for (var fieldIndex = 0; fieldIndex < channel.ChildNodes.Count; ++ fieldIndex) {
                var field = channel.ChildNodes[fieldIndex];
                switch (field.Name) {
                    case NODE_CHANNEL_ID:
                        channelInfo.SetId(Convert.ToInt64(field.InnerText));
                        break;

                    case NODE_CHANNEL_NAME:
                        channelInfo.SetName(field.InnerText);
                        break;

                    case NODE_CHANNEL_NUMBER_ON_DEVICE:
                        channelInfo.SetChannelNumber(Convert.ToInt32(field.InnerText));
                        break;

                    case NODE_CHANNEL_GROUP:
                        channelInfo.SetGroupId(Convert.ToInt64(field.InnerText));
                        break;

                    default:
                        throw new XmlException(string.Format("Unknown item {0} in channel configuration", field.Name));
                }
            }

            outputChannels.Add(channelInfo);
        }

        /// <summary>
        /// Ищет описание устройства вывода по его идентификатору.
        /// </summary>
        /// <param name="aDeviceId">Идентификатор устройства.</param>
        /// <returns>Описание устройств ввода. Описание устройств ввода. <code>null<code>, если не найден.</returns>
        public OutputDeviceInfo FindOutputDeviceById(long aDeviceId)
        {
            for (int i = 0; i < outputDevices.Count(); i++) {
                OutputDeviceInfo outputDevice = outputDevices.ElementAt(i);
                if (outputDevice.GetId() == aDeviceId) {
                    return outputDevice;
                }
            }
            return null;
        }

        /// <summary>
        /// Ищет описание устройств ввода по его имени.
        /// </summary>
        /// <param name="aDeviceName"></param>
        /// <returns>Описание устройств ввода. <code>null<code>, если не найден.</returns>
        public OutputDeviceInfo FindOutputDeviceByName(string aDeviceName)
        {
            for (int i = 0; i < outputDevices.Count(); i++) {
                OutputDeviceInfo outputDevice = outputDevices.ElementAt(i);
                if (outputDevice.GetName().Equals(aDeviceName)) {
                    return outputDevice;
                }
            }
            return null;
        }

        /// <summary>
        /// Возвращает количество описаний устройств ввода.
        /// </summary>
        /// <returns>Количество описаний устройств ввода.</returns>
        public int GetOutputDevicesCount()
        {
            return outputDevices.Count();
        }

        /// <summary>
        /// Возвращает описание устройства ввода по индексу.
        /// </summary>
        /// <param name="aIndex">Индекс.</param>
        /// <returns>Описание устройств ввода.</returns>
        /// <exception cref="IndexOutOfRangeException">Неверный индекс. Допустимый диаппазон значений [0 .. <see cref="GetOutputDevicesCount()"/> - 1]></exception>
        public OutputDeviceInfo GetOutputDevice(int aIndex)
        {
            return outputDevices.ElementAt(aIndex);
        }

        /// <summary>
        /// Ищет описание канала по его идентификатору.
        /// </summary>
        /// <param name="aChannelId">Идентификатор канала.</param>
        /// <returns>Описание канала. <code>null</code>, если не найден.</returns>
        public OutputChannelInfo FindChannelById(long aChannelId)
        {
            for (int i = 0; i < outputChannels.Count(); i++) {
                OutputChannelInfo channel = outputChannels.ElementAt(i);
                if (channel.GetId() == aChannelId) {
                    return channel;
                }
            }
            return null;
        }

        /// <summary>
        /// Ищет описание канала по его имени.
        /// </summary>
        /// <param name="aChannelName"></param>
        /// <returns>Описание устройств ввода. <code>null</code>, если не найден.</returns>
        public OutputChannelInfo FindChannelByName(string aChannelName)
        {
            for (int i = 0; i < outputChannels.Count(); i++) {
                OutputChannelInfo channel = outputChannels.ElementAt(i);
                if (channel.GetName().CompareTo(aChannelName) == 0) {
                    return channel;
                }
            }
            return null;
        }

        /// <summary>
        /// Возвращает количество описаний каналов.
        /// </summary>
        /// <returns>Количество описаний каналов.</returns>
        public int GetOutChannelsCount()
        {
            return outputChannels.Count();
        }

        /// <summary>
        /// Возвращает описание канала по индексу.
        /// </summary>
        /// <param name="aIndex">Индекс.</param>
        /// <returns>Описание канала.</returns>
        /// <exception cref="IndexOutOfRangeException">Неверный индекс. Допустимый диаппазон значений [0 .. <see cref="GetOutChannelsCount()"/> - 1]></exception>
        public OutputChannelInfo GetOutChannel(int aIndex)
        {
            return outputChannels.ElementAt(aIndex);
        }

        /// <summary>
        /// Ищет описание групы каналов по его идентификатору.
        /// </summary>
        /// <param name="aChannelGroupId">Идентификатор группы каналов.</param>
        /// <returns>Описание группы каналов. <code>null</code>, если не найден.</returns>
        public ChannelGroupInfo FindChannelGroupById(long aChannelGroupId)
        {
            for (int i = 0; i < channelGroups.Count(); i++) {
                ChannelGroupInfo channelGroup = channelGroups.ElementAt(i);
                if (channelGroup.GetId() == aChannelGroupId) {
                    return channelGroup;
                }
            }
            return null;
        }

        /// <summary>
        /// Возвращает количество описаний групп каналов.
        /// </summary>
        /// <returns>Количество описаний каналов.</returns>
        public int GetChannelGroupsCount()
        {
            return channelGroups.Count();
        }

        /// <summary>
        /// Возвращает описание группы каналов по индексу.
        /// </summary>
        /// <param name="aIndex">Индекс.</param>
        /// <returns>Описание группы каналов.</returns>
        /// <exception cref="IndexOutOfRangeException">Неверный индекс. Допустимый диаппазон значений [0 .. <see cref="GetChannelGroupsCount()"/> - 1]></exception>
        public ChannelGroupInfo GetChannelGroup(int aIndex)
        {
            return channelGroups.ElementAt(aIndex);
        }

        /// <summary>
        /// Подготавливает загруженную конфигурацию к использованию.
        /// </summary>
        private void PrepareConfiguration()
        {
            for (var i = 0; i < outputChannels.Count(); i++) {
                var channel = outputChannels.ElementAt(i);                
                // Найдем для канала соответсвующее устройство вывода.
                var outputDevice = (OutputDeviceInfoImpl)FindOutputDeviceById(channel.GetDeviceId());
                if (outputDevice == null) {
                    // Устройство вывода не найдено.
                    throw new Exception("Устройство вывода не найдено. Id = " + channel.GetDeviceId());
                }
                // Связываем канал с устройством вывода.
                channel.SetDevice(outputDevice);
                outputDevice.GetChannels().Add(channel);

                // Найдем для канала соответсвующую группу.
                var channelGroup = (ChannelGroupInfoImpl)FindChannelGroupById(channel.GetGroupId());
                if (channelGroup == null) {
                    // Группа не найдена.
                    // Создаем новую группу.
                    channelGroup = new ChannelGroupInfoImpl();
                    channelGroup.SetId(channel.GetGroupId());
                    channelGroups.Add(channelGroup);
                    Logger.Info("Конфигурация сервера: Создана группа каналов. Идентификатор = " + channelGroup.GetId());
                }
                // Связываем канал с группой каналов.
                channel.SetGroup(channelGroup);
                channelGroup.GetChannels().Add(channel);
            }

            for (var i = 0; i < inputChannels.Count; ++i) {
                var channel = inputChannels[i];
                // Найдем для канала соответсвующее устройство вывода.
                var inputDevice = (InputDeviceInfoImpl) FindInputDeviceById(channel.GetDeviceId());
                if (inputDevice == null) {
                    // Устройство вывода не найдено.
                    throw new Exception("Устройство ввода не найдено. Id = " + channel.GetDeviceId());
                }
                // Связываем канал с устройством вывода.
                channel.SetDevice(inputDevice);
                inputDevice.GetChannels().Add(channel);
            }
        }

        /// <summary>
        /// Выполняет поиск устройства ввода по идентификатору.
        /// </summary>
        /// <param name="aDeviceId">Идентификатор устройства ввода.</param>
        /// <returns>Найденное устройство ввода или null.</returns>
        private InputDeviceInfo FindInputDeviceById(long aDeviceId)
        {
            return inputDevices.FirstOrDefault(device => device.GetId() == aDeviceId);
        }

        /// <summary>
        /// Возвращает число устройств ввода.
        /// </summary>
        /// <returns>Число устройств ввода.</returns>
        public int GetInputDevicesCount()
        {
            return inputDevices.Count;
        }

        /// <summary>
        /// Возвращает устройство ввода по индексу.
        /// </summary>
        /// <param name="i">Индекс в списке устройств ввода.</param>
        /// <returns>Устройство ввода.</returns>
        public InputDeviceInfo GetInputDevice(int i)
        {
            return inputDevices[i];
        }

        /// <summary>
        /// Возвращает количество входных каналов.
        /// </summary>
        /// <returns>Число входных каналов.</returns>
        public int GetInChannelsCount()
        {
            return inputChannels.Count;
        }

        /// <summary>
        /// Возвращает входной канал по индексу в списке входных каналов.
        /// </summary>
        /// <param name="i">Индекс.</param>
        /// <returns>Входной канал.</returns>
        public InputChannelInfo GetInChannel(int i)
        {
            return inputChannels[i];
        }
    }
}
