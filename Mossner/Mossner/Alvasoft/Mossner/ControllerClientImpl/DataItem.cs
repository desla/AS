using System;
using Alvasoft.ConnectionHolder;
using Alvasoft.Utils.Activity;
using log4net;
using OPCAutomation;

namespace Alvasoft.Mossner.ControllerClientImpl
{
    /// <summary>
    /// Обертка для OPC-тега.
    /// </summary>
    public class DataItem : InitializableImpl
    {
        private static readonly ILog Logger = LogManager.GetLogger("Item");

        private OpcConnectionHolder opcConnection;        

        private Array serverHandles;
        private Array values;
        private Array errors;
        private object quality;
        private object timeStamp;

        public string Name { get; private set; }
        public OPCGroup Group { get; private set; }
        public OPCItem Item { get; private set; }    

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aName"></param>
        /// <param name="aOpcConnection"></param>
        public DataItem(string aName, OpcConnectionHolder aOpcConnection)
        {
            Name = aName;
            opcConnection = aOpcConnection;
        }        
        
        /// <summary>
        /// Инициализация.
        /// </summary>
        protected override void DoInitialize()
        {
            try {
                Logger.Debug("Инициализация тега " + Name + "...");
                var server = opcConnection.GetOpcServer();
                Group = server.OPCGroups.Add(Guid.NewGuid().ToString());
                Item = Group.OPCItems.AddItem(Name, 0);
                serverHandles = new[] { 0, Item.ServerHandle };
                Logger.Debug("Инициализация тега " + Name + " завершена.");
            }
            catch (Exception ex) {
                opcConnection.ProcessError(ex);
            }
        }

        /// <summary>
        /// Деинициализация.
        /// </summary>
        protected override void DoUninitialize()
        {
            try {
                Logger.Debug("Деинициализация тега " + Name + "...");
                if (opcConnection.IsConnected()) {
                    var server = opcConnection.GetOpcServer();
                    server.OPCGroups.Remove(Group.Name);
                }
                Logger.Debug("Деинициализация тега " + Name + " завершена.");
            }
            catch (Exception ex) {
                opcConnection.ProcessError(ex);
            }
        }

        /// <summary>
        /// Записывает значение тега.
        /// </summary>
        /// <param name="value">Новое значение.</param>
        public void Write(object value)
        {
            if (!IsInitialized()) {
                throw new Exception("Тег " + Name + " не инициализирован для записи.");
            }

            try {
                Item.Write(value);
            }
            catch (Exception ex) {
                Logger.Error("Ошибка записи тега " + Name);
            }
        }

        /// <summary>
        /// Читает текущее значение OPC-тега.
        /// </summary>
        /// <returns>Значение тега.</returns>
        public object Read()
        {
            if (!IsInitialized()) {
                throw new Exception("Тег " + Name + " не инициализирован для чтения.");
            }            
            try {
                Group.SyncRead((short)OPCDataSource.OPCCache,
                                  1,
                                  ref serverHandles,
                                  out values,
                                  out errors,
                                  out quality,
                                  out timeStamp);

                return values.GetValue(1);
            }
            catch (Exception ex) {
                Logger.Error(string.Format("Ошибка чтения тега {0}: {1}", Name, ex.Message));
            }

            throw new FormatException();
        }
    }
}
