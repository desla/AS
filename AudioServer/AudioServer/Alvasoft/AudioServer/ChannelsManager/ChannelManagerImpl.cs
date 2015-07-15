using System;
using System.Collections.Generic;
using Alvasoft.AudioServer.ChannelsManager.Impl.Devices;
using Alvasoft.AudioServer.Configuration;
using Alvasoft.AudioServer.ChannelsManager.Impl;
using AudioServer.Alvasoft.AudioServer.ChannelsManager.Impl.Devices;

namespace Alvasoft.AudioServer.ChannelsManager
{
    /// <summary>
    /// Знает устройство инфраструктуры устройства Вывода + каналы + группы.
    /// </summary>
    public class ChannelManagerImpl : IChannelManager, IDisposable
    {
        private readonly List<IOutputDevice> outputDevices = new List<IOutputDevice>();
        private readonly List<OutputChannel> outChannels = new List<OutputChannel>();        
        private readonly List<ChannelGroup> groups = new List<ChannelGroup>();

        private readonly List<IInputDevice> inputDevices = new List<IInputDevice>();
        private readonly List<InputChannel> inChannels = new List<InputChannel>();        

        private ServerConfiguration configuration;

        /// <summary>
        /// Реализация менеджера каналов.
        /// </summary>
        /// <param name="aConfiguration">Конфигурация.</param>
        public ChannelManagerImpl(ServerConfiguration aConfiguration)
        {
            if (aConfiguration == null) {
                throw new ArgumentNullException("aConfiguration");
            }

            configuration = aConfiguration;
            Configure();
        }        

        /// <summary>
        /// Запускает каналы в работу.
        /// </summary>
        public void StartManage()
        {            
            foreach (var channel in outChannels) {
                channel.BeginWork();
            }
        }

        /// <summary>
        /// Останавливает работу каналов.
        /// </summary>
        public void StopManage()
        {
            foreach (var channel in outChannels) {
                channel.StopWork();
            }
        }

        public OutputChannel GetChannelById(int aChannelId)
        {
            foreach (var channel in outChannels) {
                if (channel.GetChannelInfo().GetId() == aChannelId) {
                    return channel;
                }
            }

            throw new ArgumentException("aChannelId");
        }

        /// <summary>
        /// Команда начала записи.
        /// </summary>
        /// <param name="aInputLineId">Номер входного канала.</param>
        /// <param name="aChannelsIds">Номеры выходных каналов.</param>
        /// <param name="aConnectionHashCode">Для идентификации иниатора записи.</param>
        public void StartRecord(int aInputLineId, int[] aChannelsIds, int aConnectionHashCode)
        {
            foreach (var inChannel in inChannels) {
                if (inChannel.GetChannelInfo().GetId() == aInputLineId) {
                    inChannel.StartRecord(aChannelsIds, aConnectionHashCode);
                    break;
                }
            }
        }

        /// <summary>
        /// Команда прекращения записи.
        /// </summary>
        /// <param name="aConnectionHashCode">Для идентификации инициатора записи.</param>
        public void StopRecord(int aConnectionHashCode)
        {
            foreach (var inChannel in inChannels) {
                inChannel.StopRecord(aConnectionHashCode);
            }
        }

        /// <summary>
        /// Отправляет сообщение в очередь указанного канала.
        /// </summary>
        /// <param name="aMessage">Сообщение.</param>
        /// <param name="aChannelId">Логический идентификатор канала.</param>
        public void ProcessMessage(SoundMessage aMessage, long aChannelId)
        {
            if (aMessage == null) {
                throw new ArgumentNullException("aMessage");
            }

            foreach (var channel in outChannels) {                
                if (channel.GetChannelInfo().GetId() == aChannelId) {
                    if (channel.IsWorking()) {
                        channel.InsertMessage(aMessage);
                    }
                    else {
                        throw new ArgumentException(string.Format("Channel {0} is not working", aChannelId));
                    }

                    return;
                }
            }

            throw new ArgumentException(string.Format("Channel {0} was not found", aChannelId));
        }

        /// <summary>
        /// Закрываем все каналы, отключаемся от устройств.
        /// </summary>
        public void Dispose()
        {
            foreach (var channel in outChannels) {
                if (channel.IsWorking()) {
                    channel.StopWork();
                }
            }

            foreach (var device in outputDevices) {
                device.Dispose();
            }

            foreach (var device in inputDevices) {
                device.Dispose();
            }
        }

        /// <summary>
        /// На основе configuration собирает перферию из девайсов + каналы + группы.
        /// </summary>
        private void Configure()
        {
            FillGroups();
            FillOutputDevices();
            FillInputDevices();
        }

        private void FillInputDevices()
        {
            var deviceCound = configuration.GetInputDevicesCount();
            for (var deviceIndex = 0; deviceIndex < deviceCound; ++deviceIndex) {
                var deviceInfo = configuration.GetInputDevice(deviceIndex);
                IInputDevice device = null;                
                switch (deviceInfo.GetDeviceType()) {
                    case DeviceType.DEFAULT:
                        device = DefaultInputDevice.FindPhysicalDeviceByName(deviceInfo.GetName());
                        break;

                    case DeviceType.ASIO:
                        device = new AsioInputDevice(deviceInfo.GetName());
                        break;
                }

                if (device == null) {
                    throw new ArgumentException(string.Format("aDevice \"{0}\" not found", deviceInfo.GetName()));
                }

                device.SetCallback(this);
                inputDevices.Add(device);

                // заполним каналы для устройства        
                FillInChannels(deviceInfo, device);
            }
        }

        /// <summary>
        /// Заполняет входные каналы.
        /// </summary>
        /// <param name="aDeviceInfo">Описание устройства ввода.</param>
        /// <param name="aDevice">Устройство ввода.</param>
        private void FillInChannels(InputDeviceInfo aDeviceInfo, IInputDevice aDevice)
        {
            var channelsCount = aDeviceInfo.GetChannelsCount();
            if (channelsCount > aDevice.GetChannelsCount()) {
                throw new ArgumentException("Сhannels count on phisical device is less than in configuration");
            }

            for (var channelIndex = 0; channelIndex < channelsCount; ++channelIndex) {
                var channelInfo = aDeviceInfo.GetChannel(channelIndex);

                var channel = new InputChannel(channelInfo, aDevice);
                inChannels.Add(channel);
            }
        }

        /// <summary>
        /// Генерируем список групп.
        /// </summary>
        private void FillGroups()
        {
            var groupCount = configuration.GetChannelGroupsCount();
            for (var groupIndex = 0; groupIndex < groupCount; ++groupIndex) {
                var groupInfo = configuration.GetChannelGroup(groupIndex);
                var group = new ChannelGroup(groupInfo);

                groups.Add(group);
            }
        }

        /// <summary>
        /// Генерируем системные устройства.
        /// </summary>
        private void FillOutputDevices()
        {
            var deviceCound = configuration.GetOutputDevicesCount();
            for (var deviceIndex = 0; deviceIndex < deviceCound; ++deviceIndex) {
                var deviceInfo = configuration.GetOutputDevice(deviceIndex);
                IOutputDevice device = null;
                switch (deviceInfo.GetDeviceType()) {
                    case DeviceType.DEFAULT:
                        device = DefaultOutputDevice.FindPhysicalDeviceByName(deviceInfo.GetName());
                        break;

                    case DeviceType.ASIO:
                        device = new AsioOutputDevice(deviceInfo.GetName());                        
                        break;
                }

                if (device == null) {
                    throw new ArgumentException(string.Format("aDevice \"{0}\" not found", deviceInfo.GetName()));
                }

                outputDevices.Add(device);

                // заполним каналы для устройства        
                FillOutChannels(deviceInfo, device);
            }
        }

        /// <summary>
        /// Генерируем каналы для заданного устройства.
        /// </summary>
        /// <param name="aDeviceInfo">OutputDeviceInfo.</param>
        /// <param name="aDevice">OutputDevice.</param>
        private void FillOutChannels(OutputDeviceInfo aDeviceInfo, IOutputDevice aDevice)
        {
            var channelsCount = aDeviceInfo.GetChannelsCount();
            if (channelsCount > aDevice.GetChannelsCount()) {
                throw new ArgumentException("Сhannels count on phisical device is less than in configuration");
            }

            for (var channelIndex = 0; channelIndex < channelsCount; ++channelIndex) {
                var channelInfo = aDeviceInfo.GetChannel(channelIndex);

                ChannelGroup channelGroup = null;
                foreach (var group in groups) {
                    if (group.GetChannelGroupInfo().GetId() == channelInfo.GetGroup().GetId()) {
                        channelGroup = group;
                        break;
                    }
                }

                if (channelGroup == null) {
                    throw new ArgumentException("Channel group was not found for channel " + channelInfo.GetId());
                }

                var channel = new OutputChannel(channelInfo, aDevice, channelGroup);
                outChannels.Add(channel);
            }
        }       
    }
}
