using System;
using System.Timers;
using Alvasoft.Utils.Activity;
using Alvasoft.WagstaffBridge.BridgeConfiguration;
using Alvasoft.WagstaffBridge.ConnectionHolder;
using log4net;
using OPCAutomation;

namespace Alvasoft.WagstaffBridge.OpcDataReader
{
    /// <summary>
    /// Читает данные из OPC сервера.
    /// </summary>
    public class DataReader : InitializableImpl
    {
        private static readonly ILog Logger = LogManager.GetLogger("DataReader");

        private DataReaderCallback callback;
        private DataReadInfo[] data;
        private OpcConnectionHolder opcConnection;
        private OPCGroup opcGroup;
        private OPCItem[] items;
        private bool active;
        private Timer readTimer = new Timer();

        /// <summary>
        /// Конструктор.
        /// </summary>
        public DataReader()
        {
            readTimer.Enabled = false;
            readTimer.Elapsed += ReadData;
        }

        /// <summary>
        /// Установить конфигурацию.
        /// </summary>
        /// <param name="aConfiguration">Конфигурация.</param>
        public void SetConfiguration(DataReaderConfiguration aConfiguration)
        {
            SetData(aConfiguration.DataInfo.ToArray());
            SetReadInterval(aConfiguration.ReadInterval);
        }        

        /// <summary>
        /// Установить callback, который будет вызываться при получении данных.
        /// </summary>
        /// <param name="aCallback">Callback.</param>
        public void SetCallback(DataReaderCallback aCallback)
        {
            callback = aCallback;
        }

        /// <summary>
        /// Установить DataReadInfo, где хранится информация о читаемых данных - идентификаторы, имена.
        /// </summary>
        /// <param name="aData">Список данных.</param>
        public void SetData(DataReadInfo[] aData)
        {
            data = aData;
        }

        /// <summary>
        /// Установить ConnectionHolder для OPC сервера.
        /// </summary>
        /// <param name="aConnection"></param>
        public void SetOpcConnectionHolder(OpcConnectionHolder aConnection)
        {
            opcConnection = aConnection;
        }

        /// <summary>
        /// Установить состояние работы.
        /// </summary>
        /// <param name="aActive">True - запустить, false - остановить.</param>
        public void SetActive(bool aActive)
        {
            if (active == aActive) {
                throw new ArgumentException();
            }

            active = aActive;
            if (active) {
                Logger.Debug("Активирован");
                readTimer.Start();
            }
            else {
                Logger.Debug("Деактивирован");
                readTimer.Stop();
            }
        }

        /// <summary>
        /// Получить текущее состояние работы.
        /// </summary>
        /// <returns>True - если активен, false - иначе.</returns>
        public bool IsActive()
        {
            return active;
        }

        /// <summary>
        /// Получить интервал чтения данных.
        /// </summary>
        /// <returns>Интервал в милиесундах.</returns>
        public double GetReadInterval()
        {
            return readTimer.Interval;
        }

        /// <summary>
        /// Установить интервал чтения данных.
        /// </summary>
        /// <param name="aInterval"></param>
        public void SetReadInterval(double aInterval)
        {
            readTimer.Interval = aInterval;
        }

        /// <summary>
        /// Прочитать данные из OPC сервера.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadData(object sender = null, ElapsedEventArgs e = null)
        {
            var numItems = data.Length;
            Array serverHandles = new int[numItems + 1];
            for (var itemIndex = 0; itemIndex < numItems; ++itemIndex) {
                serverHandles.SetValue(items[itemIndex].ServerHandle, itemIndex + 1);
            }

            Array values;
            Array errors;
            object quality;
            object timeStamp;

            Logger.Debug("Чтение данных");

            try {
                opcGroup.SyncRead((short) OPCDataSource.OPCCache, numItems, ref serverHandles, out values, out errors,
                    out quality, out timeStamp);
                opcConnection.UpdateLastOperationTime();
            }
            catch (Exception ex) {
                Logger.Error(ex);
                return;
            }

            Logger.Debug("Данные прочитаны. Количество: " + values.Length);

            try {
                for (var valueIndex = 0; valueIndex < values.Length; ++valueIndex) {
                    var dataValue = new DataValue();
                    dataValue.TypeId = data[valueIndex].TypeId;
                    dataValue.DataId = data[valueIndex].DataId;
                    dataValue.ObjectId = data[valueIndex].ObjectId;
                    dataValue.Value = double.Parse(values.GetValue(valueIndex + 1).ToString());
                    dataValue.ValueTime = DateTime.Now;
                    if (callback != null) {
                        callback.OnReadedData(dataValue);
                    }
                }
            }
            catch (Exception ex) {
                Logger.Error("Ошибка значения из OPC сервера.");
            }
        }       

        /// <summary>
        /// Инициализация. Сосздание группы, ее настройка.
        /// </summary>
        protected override void DoInitialize()
        {
            try {
                Logger.Debug("Инициализация");

                var server = opcConnection.GetOpcServer();
                opcGroup = server.OPCGroups.Add(Guid.NewGuid().ToString());
                items = new OPCItem[data.Length];
                for (var dataIndex = 0; dataIndex < data.Length; ++dataIndex) {
                    var item = opcGroup.OPCItems.AddItem(data[dataIndex].OpcItemName, dataIndex);
                    items[dataIndex] = item;
                }
            }
            catch (Exception ex) {
                opcConnection.ProcessError(ex);
            }
        }

        /// <summary>
        /// Деинициализация. Удаление группы тегов.
        /// </summary>
        protected override void DoUninitialize()
        {
            try {
                Logger.Debug("Деинициализация");
                if (opcConnection.IsConnected()) {
                    var server = opcConnection.GetOpcServer();
                    server.OPCGroups.Remove(opcGroup.Name);
                }
            }
            catch (Exception ex) {
                opcConnection.ProcessError(ex);
            }
        }
    }
}
