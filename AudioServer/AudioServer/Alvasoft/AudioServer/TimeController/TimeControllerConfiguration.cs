using System;
using System.Xml;

namespace Alvasoft.AudioServer.TimesController
{
    /// <summary>
    /// Конфигуратор контроллера времени.
    /// </summary>
    public class TimeControllerConfiguration
    {
        private const string NODE_MINUTES = "everyMinutes";
        private const string NODE_CHANNELS = "channels";
        private const string NODE_CHANNEL_ID = "channelId";

        /// <summary>
        /// Значение, в минуту кратную которому, будет объявляться текущее время.
        /// Например, "15" - время будет объявляться в каждые 0, 15, 30, 45.. минут каждого часа.
        /// Например, "60" - время будет объявляться каждый час.
        /// </summary>
        private int minutes;
        /// <summary>
        /// Логические номера каналов, по которым будет объявляться время, через каждые minutes.
        /// </summary>
        private int[] channelIds;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aNode">XmlNode с настройками.</param>
        public TimeControllerConfiguration(XmlNode aNode)
        {
            var items = aNode.ChildNodes;
            for (var itemIndex = 0; itemIndex < items.Count; ++itemIndex) {
                switch (items[itemIndex].Name) {
                    case NODE_MINUTES:
                        minutes = Convert.ToInt32(items[itemIndex].InnerText);
                        break;
                    case NODE_CHANNELS:
                        LoadChannelsFromXmlNode(items[itemIndex]);
                        break;
                }
            }
        }

        /// <summary>
        /// Загружает конфигурацию из XmlNode.
        /// </summary>
        /// <param name="aNode">XmlNode, содержащий настройки.</param>
        private void LoadChannelsFromXmlNode(XmlNode aNode)
        {
            var items = aNode.ChildNodes;
            channelIds = new int[items.Count];
            for (var itemIndex = 0; itemIndex < items.Count; ++itemIndex) {
                switch (items[itemIndex].Name) {
                    case NODE_CHANNEL_ID:
                        channelIds[itemIndex] = Convert.ToInt32(items[itemIndex].InnerText);
                        break;
                    default:
                        throw new ArgumentException("Неизвестный параметр: " + items[itemIndex].Name);
                }
            }
        }

        /// <summary>
        /// Возвращает минуты, являющиеся кратнами для объявления времени.
        /// </summary>
        /// <returns></returns>
        public int GetMinutes()
        {
            return minutes;
        }

        /// <summary>
        /// Идентификаторы выходных каналов для объявления времени.
        /// </summary>
        /// <returns></returns>
        public int[] GetChannelsIds()
        {
            return channelIds;
        }
    }
}
