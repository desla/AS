using System;
using Alvasoft.Utils.Activity;
using Alvasoft.WagstaffBridge.ConnectionHolder;
using log4net;
using OPCAutomation;

namespace Alvasoft.WagstaffBridge.OpcDataReader
{
    /// <summary>
    /// Активатор для DataReader. Настраивается на указанный тег в OPC. Для определения начала и конца плавки.
    /// </summary>
    public class DataReaderActivator : InitializableImpl
    {
        private static readonly ILog Logger = LogManager.GetLogger("DataReaderActivator");

        private OpcConnectionHolder opcConnection;
        private DataReader dataReader;
        private string activationItemName;
        private OPCGroup opcGroup;
        private OPCItem opcItem;

        /// <summary>
        /// Установить ConnectionHolder для OPC сервера.
        /// </summary>
        /// <param name="aOpcConnection">ConnectionHolder.</param>
        public void SetOpcConnection(OpcConnectionHolder aOpcConnection)
        {
            opcConnection = aOpcConnection;
        }

        /// <summary>
        /// Установить DataReader, активируемый в случае начала плавки.
        /// </summary>
        /// <param name="aDataReader">DataReader.</param>
        public void SetDataReader(DataReader aDataReader)
        {
            dataReader = aDataReader;
        }

        /// <summary>
        /// Установить имя тега, значение которого мониторится.
        /// SYS_PV_StatePos.
        /// </summary>
        /// <param name="aItemName">Имя тега.</param>
        public void SetActivationItemName(string aItemName)
        {
            activationItemName = aItemName;
        }

        /// <summary>
        /// Инициализация. Настройка группы, установка асинхронного метода чтения данных для указанного тега.
        /// </summary>
        protected override void DoInitialize()
        {
            try {
                Logger.Debug("Инициализация");
                var server = opcConnection.GetOpcServer();
                opcGroup = server.OPCGroups.Add(Guid.NewGuid().ToString());
                opcItem = opcGroup.OPCItems.AddItem(activationItemName, 0);
                opcGroup.UpdateRate = 100;
                opcGroup.IsActive = true;
                opcGroup.IsSubscribed = true;
                opcGroup.DataChange += ActivationItemValueChanged;                

                dataReader.Initialize();

                ReadItemFirstTime();
            }
            catch (Exception ex) {
                opcConnection.ProcessError(ex);
            }
        }

        /// <summary>
        /// Получить значение тега в первый раз (при запуске).
        /// </summary>
        private void ReadItemFirstTime()
        {
            Array serverHandles = new[] { 0, opcItem.ServerHandle };

            Array values;
            Array errors;
            object quality;
            object timeStamp;

            try {
                opcGroup.SyncRead((short)OPCDataSource.OPCCache, 1, ref serverHandles, out values, out errors, out quality, out timeStamp);
                ActivateReader(int.Parse(values.GetValue(1).ToString()));
            }
            catch (Exception ex) {
                Logger.Info("Ошибка: тег активации не найден");
            }
        }

        /// <summary>
        /// Деинициализация. Закрытие соединения, очистка группы OPC.
        /// </summary>
        protected override void DoUninitialize()
        {
            try {
                Logger.Debug("Деинициализация");
                if (opcConnection.IsConnected()) {
                    var server = opcConnection.GetOpcServer();
                    server.OPCGroups.Remove(opcGroup.Name);
                }

                dataReader.Uninitialize();
            }
            catch (Exception ex) {
                opcConnection.ProcessError(ex);
            }
        }

        /// <summary>
        /// Асинхронное событие происходит при изменении значения указанного OPC-тега.
        /// </summary>
        /// <param name="transactionid">Номер транзакции.</param>
        /// <param name="numitems">Количество элементов группы.</param>
        /// <param name="clienthandles">Список handle'ов группы.</param>
        /// <param name="itemvalues">Значения тегов.</param>
        /// <param name="qualities">Качество данных.</param>
        /// <param name="timestamps">Время изменения.</param>
        private void ActivationItemValueChanged(int transactionid, int numitems, ref Array clienthandles, ref Array itemvalues, ref Array qualities, ref Array timestamps)
        {
            try {
                var activationItemValue = int.Parse(itemvalues.GetValue(1).ToString());
                ActivateReader(activationItemValue);
            }
            catch (Exception ex) {
                Logger.Info("Ошибка: Тег активации DataReader не найден");
            }
        }

        /// <summary>
        /// Активировать, либо деактивировать DataReader с учетом значения тега.
        /// </summary>
        /// <param name="aActivationItemValue">Значение тега.</param>
        private void ActivateReader(int aActivationItemValue)
        {
            opcConnection.UpdateLastOperationTime();
            if (aActivationItemValue >= 3 && aActivationItemValue <= 10) {
            //if (aActivationItemValue != 0) {
                Logger.Info("Процесс литья активен. " + activationItemName + " имеет значение " + aActivationItemValue);
                if (!dataReader.IsActive()) {
                    dataReader.SetActive(true);
                }
            }
            else {
                Logger.Info("Процесс литья не активен." + activationItemName + " имеет значение " + aActivationItemValue);
                if (dataReader.IsActive()) {
                    dataReader.SetActive(false);
                }
            }   
        }
    }
}
