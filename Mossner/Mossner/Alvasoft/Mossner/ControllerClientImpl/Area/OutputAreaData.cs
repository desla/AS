using Alvasoft.ConnectionHolder;
using log4net;

namespace Alvasoft.Mossner.ControllerClientImpl.Area
{
    /// <summary>
    /// Область выходных слитков.
    /// </summary>
    public class OutputAreaData : AbstractAreaData2
    {
        private static readonly ILog Logger = LogManager.GetLogger("InputAreaData");

        private const string ITEMID_OUTPUTAREA_ID_CHECK_OUT = "L3_Interface.OUT.OutArea_ID_Check";
        private const string ITEMID_OUTPUTAREA_ID_NUMBER_OUT = "L3_Interface.OUT.OutArea_ID_Number";
        private const string ITEMID_OUTPUTAREA_WEIGHT_OUT = "L3_Interface.OUT.OutArea_Weight";
        private const string ITEMID_OUTPUTAREA_LENGTH_OUT = "L3_Interface.OUT.OutArea_Length";
        private const string ITEMID_OUTPUTAREA_ID_CHECK_IN = "L3_Interface.IN.OutArea_ID_CheckResult";
        private const string ITEMID_OUTPUTAREA_ID_NUMBER_IN = "L3_Interface.IN.OutArea_ID_Number";

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aTopicName">Имя топика.</param>
        /// <param name="aOpcConnection">Соединение с OPC-сервером.</param>
        public OutputAreaData(string aTopicName, OpcConnectionHolder aOpcConnection)
        {
            IdCheckOutputItem = new DataItem(aTopicName + ITEMID_OUTPUTAREA_ID_CHECK_OUT, aOpcConnection);
            IdNumberOutputItem = new DataItem(aTopicName + ITEMID_OUTPUTAREA_ID_NUMBER_OUT, aOpcConnection);
            IdCheckInputItem = new DataItem(aTopicName + ITEMID_OUTPUTAREA_ID_CHECK_IN, aOpcConnection);
            IdNumberInputItem = new DataItem(aTopicName + ITEMID_OUTPUTAREA_ID_NUMBER_IN, aOpcConnection);
            WeightOutputItem = new DataItem(aTopicName + ITEMID_OUTPUTAREA_WEIGHT_OUT, aOpcConnection);
            LengthOutputItem = new DataItem(aTopicName + ITEMID_OUTPUTAREA_LENGTH_OUT, aOpcConnection);
        }

        /// <summary>
        /// Инициализация.
        /// </summary>
        public void Initialize()
        {
            Logger.Info("Инициализация OutputAreaData...");
            IdCheckOutputItem.Initialize();
            IdNumberOutputItem.Initialize();
            IdCheckInputItem.Initialize();
            IdNumberInputItem.Initialize();
            WeightOutputItem.Initialize();
            LengthOutputItem.Initialize();
            Logger.Info("Инициализация OutputAreaData завершена.");
        }

        /// <summary>
        /// Деинициализаиця.
        /// </summary>
        public void Uninitialize()
        {
            Logger.Info("Деинициализация OutputAreaData...");
            IdCheckOutputItem.Uninitialize();
            IdNumberOutputItem.Uninitialize();
            IdCheckInputItem.Uninitialize();
            IdNumberInputItem.Uninitialize();
            WeightOutputItem.Uninitialize();
            LengthOutputItem.Uninitialize();
            Logger.Info("Деинициализация OutputAreaData завершена.");
        }
    }
}
