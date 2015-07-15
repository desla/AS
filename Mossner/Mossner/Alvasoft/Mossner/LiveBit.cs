using System;
using System.Threading;
using Alvasoft.ConnectionHolder;
using Alvasoft.Utils.Activity;
using log4net;
using OPCAutomation;

namespace Alvasoft.Mossner
{
    /// <summary>
    /// Задача поддержания связи с контроллерами.
    /// </summary>
    public class LiveBit : InitializableImpl
    {
        private static readonly ILog Logger = LogManager.GetLogger("LiveBit");

        private const string ITEMID_LIVEBIT_IN = "L3_Interface.IN.LiveBit";
        private const string ITEMID_LIVEBIT_OUT = "L3_Interface.OUT.LiveBit";

        private string topicName;
        private OpcConnectionHolder opcConnection;
        private OPCGroup inGroup;
        private OPCGroup outGroup;
        private OPCItem inItem;
        private OPCItem outItem;

        private DateTime lastChangeTime = DateTime.MinValue;
        private Thread liveBitTestThread;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aTopicName"></param>
        public LiveBit(string aTopicName)
        {
            topicName = aTopicName;
        }

        /// <summary>
        /// Устанавливает соединение с OPC-сервером.
        /// </summary>
        /// <param name="aOpcConnection"></param>
        public void SetOpcConnection(OpcConnectionHolder aOpcConnection)
        {
            opcConnection = aOpcConnection;
        }

        /// <summary>
        /// Инициализация.
        /// </summary>
        protected override void DoInitialize()
        {
            try {
                Logger.Debug("Инициализация LiveBit...");
                var server = opcConnection.GetOpcServer();
                inGroup = server.OPCGroups.Add(Guid.NewGuid().ToString());
                outGroup = server.OPCGroups.Add(Guid.NewGuid().ToString());
                inItem = inGroup.OPCItems.AddItem(topicName + ITEMID_LIVEBIT_IN, 0);
                outItem = outGroup.OPCItems.AddItem(topicName + ITEMID_LIVEBIT_OUT, 0);

                outGroup.UpdateRate = 100;
                outGroup.IsActive = true;
                outGroup.IsSubscribed = true;
                outGroup.DataChange += OutDataChanged;

                liveBitTestThread = new Thread(LiveBitTest);
                liveBitTestThread.Start();

                Logger.Debug("Инициализация LiveBit завершена.");
            }
            catch (Exception ex) {
                opcConnection.ProcessError(ex);
            }
        }

        /// <summary>
        /// Тестирует соединение с OPC-сервером.
        /// </summary>
        private void LiveBitTest()
        {
            while (true) {
                if (!IsInitialized()) {
                    return;
                }

                if (DateTime.Now > lastChangeTime.AddSeconds(5)) {
                    Logger.Info("Замер LiveBit.");
                    ChangeLiveBit();
                }

                Thread.Sleep(5000);
            }            
        }

        /// <summary>
        /// Деинициализация.
        /// </summary>
        protected override void DoUninitialize()
        {
            try {
                Logger.Debug("Деинициализация LiveBit...");
                if (opcConnection.IsConnected()) {
                    var server = opcConnection.GetOpcServer();
                    server.OPCGroups.Remove(inGroup.Name);
                    server.OPCGroups.Remove(outGroup.Name);
                }
                liveBitTestThread.Abort();
                Logger.Debug("Деинициализация LiveBit завершена.");
            }
            catch (Exception ex) {
                opcConnection.ProcessError(ex);
            }
        }

        /// <summary>
        /// Отрабатывает при изменеии Out тега.
        /// </summary>
        /// <param name="transactionid">Транзакция.</param>
        /// <param name="numitems">Количество параметров.</param>
        /// <param name="clienthandles">Клиенты.</param>
        /// <param name="itemvalues">Значения тегов.</param>
        /// <param name="qualities">Качество.</param>
        /// <param name="timestamps">Время изменения.</param>
        private void OutDataChanged(int transactionid, int numitems, ref Array clienthandles, ref Array itemvalues, ref Array qualities, ref Array timestamps)
        {
            var value = Convert.ToInt32(itemvalues.GetValue(1));
            Logger.Info("Изменилось состояние LiveBitOut. Текущее значение: " + value);
            ChangeLiveBit();
        }

        /// <summary>
        /// Меняет значение In-тега.
        /// </summary>
        private void ChangeLiveBit()
        {
            var outItemValue = ReadOutItem();

            try {                
                if (outItemValue != -1) {
                    inItem.Write(outItemValue == 0 ? 1 : 0);
                    lastChangeTime = DateTime.Now;
                }                
            }
            catch (Exception ex) {
                Logger.Error("Ошибка записи тега LiveBitIn: " + ex.Message);
            }             
        }

        /// <summary>
        /// Получает значение OUT-тега.
        /// </summary>
        /// <returns></returns>
        private int ReadOutItem()
        {
            Array serverHandles = new[] { 0, outItem.ServerHandle };

            Array values;
            Array errors;
            object quality;
            object timeStamp;

            try {
                outGroup.SyncRead((short)OPCDataSource.OPCCache, 
                                  1, 
                                  ref serverHandles, 
                                  out values, 
                                  out errors, 
                                  out quality, 
                                  out timeStamp);

                return int.Parse(values.GetValue(1).ToString());
            }
            catch (Exception ex) {
                Logger.Error("Ошибка чтения тега LiveBitOut: " + ex.Message);
            }

            return -1;
        }
    }
}
