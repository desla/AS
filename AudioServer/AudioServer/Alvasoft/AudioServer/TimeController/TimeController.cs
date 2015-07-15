using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using Alvasoft.Utils.Activity;
using log4net;
using Timer = System.Timers.Timer;

namespace Alvasoft.AudioServer.TimesController
{
    /// <summary>
    /// Контроллер времени. Посылает команды Аудиосерверу для объявления времени.
    /// </summary>
    public class TimeController : InitializableImpl
    {
        private static readonly ILog Logger = LogManager.GetLogger("TimeController");

        private const string NODE_CONFIGURATION = "configuration";

        private TimeControllerCallback callback;
        private List<TimeControllerConfiguration> configurations = new List<TimeControllerConfiguration>();

        private HashSet<int> currentChannelsIds = new HashSet<int>();        
        private int lastUsedMinute = -1;

        private Timer timeChecker;

        /// <summary>
        /// Инициализирует контролле времени.
        /// </summary>
        protected override void DoInitialize()
        {
            Logger.Info("Инициализация TimeController...");
            timeChecker = new Timer(1000);
            timeChecker.Elapsed += CheckCurrentTime;
            timeChecker.Start();
            Logger.Info("Инициализация TimeController завершена.");
        }

        /// <summary>
        /// Проверяет текущее время. Если необходимо его объявить, посылает команду.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckCurrentTime(object sender, ElapsedEventArgs e)
        {
            var currentTime = DateTime.Now;
            var currentMinute = currentTime.Hour*60 + currentTime.Minute;
            
            if (lastUsedMinute == currentMinute) {
                return;
            }

            lastUsedMinute = currentMinute;
            currentChannelsIds.Clear();

            foreach (var config in configurations) {
                if (currentMinute%config.GetMinutes() == 0) {
                    // набираем список каналов, по которым нужно произнести время.
                    var channelsIds = config.GetChannelsIds();
                    foreach (var id in channelsIds) {
                        currentChannelsIds.Add(id);
                    }                    
                }
            }            
                        
            if (currentChannelsIds.Count > 0) {
                Logger.Info("Объявление времени.");
                Logger.Info("Каналов для объявления времени: " + currentChannelsIds.Count);
                if (callback != null) {
                    var timeMessage = TimeToTextConverter.Convert(currentTime);
                    callback.OnTimeAnnounce(currentChannelsIds.ToArray(), 255, timeMessage);
                }
            }                            
        }

        /// <summary>
        /// Деинициализирует контроллер времени.
        /// </summary>
        protected override void DoUninitialize()
        {
            Logger.Info("Деинициализация TimeController...");
            timeChecker.Stop();
            timeChecker.Dispose();
            Logger.Info("Деинициализация TimeController завершена.");
        }

        /// <summary>
        /// Загружает конфигурацию из файла.
        /// </summary>
        /// <param name="aXmlFile">Путь до xml-файла.</param>
        public void LoadFromXmlFile(string aXmlFile)
        {
            if (string.IsNullOrEmpty(aXmlFile)) {
                throw new ArgumentNullException("aXmlFile");
            }

            var filePath = aXmlFile;
            if (!Path.IsPathRooted(aXmlFile)) {
                filePath = Application.StartupPath + "\\" + aXmlFile;
            }

            var document = new XmlDocument();
            document.Load(filePath);

            var root = document.DocumentElement;

            if (root == null) {
                throw new XmlException(filePath + " is not contain root element");
            }

            var items = root.ChildNodes;
            for (var itemIndex = 0; itemIndex < items.Count; ++itemIndex) {
                switch (items[itemIndex].Name) {
                    case NODE_CONFIGURATION:
                        var configuration = new TimeControllerConfiguration(items[itemIndex]);
                        configurations.Add(configuration);
                        break;
                    default:
                        throw new ArgumentException("Неизвестный параметр: " + items[itemIndex].Name);
                }
            }
        }

        /// <summary>
        /// Устанавливает callback для обратной связи.
        /// </summary>
        /// <param name="aCallback"></param>
        public void SetCallback(TimeControllerCallback aCallback)
        {
            callback = aCallback;
        }
    }
}
