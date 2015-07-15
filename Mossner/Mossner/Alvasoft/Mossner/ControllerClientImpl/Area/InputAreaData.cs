using Alvasoft.ConnectionHolder;
using log4net;

namespace Alvasoft.Mossner.ControllerClientImpl.Area
{
    /// <summary>
    /// Область входных слитков.
    /// </summary>
    public class InputAreaData : AbstractAreaData
    {
        private static readonly ILog Logger = LogManager.GetLogger("InputAreaData");

        private const string ITEMID_INPUTAREA_ID_CHECK_OUT = "L3_Interface.OUT.InArea_ID_Check";
        private const string ITEMID_INPUTAREA_ID_NUMBER_OUT = "L3_Interface.OUT.InArea_ID_Number";
        private const string ITEMID_INPUTAREA_ID_CHECK_IN = "L3_Interface.IN.InArea_ID_CheckResult";
        private const string ITEMID_INPUTAREA_ID_NUMBER_IN = "L3_Interface.IN.InArea_ID_Number";   

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aTopicName">Имя топика для OPC-тегов.</param>
        /// <param name="aOpcConnection">Соединеие с OPC-сервером.</param>
        public InputAreaData(string aTopicName, OpcConnectionHolder aOpcConnection)
        {
            IdCheckOutputItem = new DataItem(aTopicName + ITEMID_INPUTAREA_ID_CHECK_OUT, aOpcConnection);
            IdNumberOutputItem = new DataItem(aTopicName + ITEMID_INPUTAREA_ID_NUMBER_OUT, aOpcConnection);
            IdCheckInputItem = new DataItem(aTopicName + ITEMID_INPUTAREA_ID_CHECK_IN, aOpcConnection);
            IdNumberInputItem = new DataItem(aTopicName + ITEMID_INPUTAREA_ID_NUMBER_IN, aOpcConnection);
        }

        /// <summary>
        /// Инициализация.
        /// </summary>
        public void Initialize()
        {
            Logger.Info("Инициализация InputAreaData...");
            IdCheckOutputItem.Initialize();
            IdNumberOutputItem.Initialize();
            IdCheckInputItem.Initialize();
            IdNumberInputItem.Initialize();
            Logger.Info("Инициализация InputAreaData завершена.");
        }

        /// <summary>
        /// Деинициализация.
        /// </summary>
        public void Uninitialize()
        {
            Logger.Info("Деинициализация InputAreaData...");
            IdCheckOutputItem.Uninitialize();
            IdNumberOutputItem.Uninitialize();
            IdCheckInputItem.Uninitialize();
            IdNumberInputItem.Uninitialize();
            Logger.Info("Деинициализация InputAreaData завершена.");
        }
    }
}
