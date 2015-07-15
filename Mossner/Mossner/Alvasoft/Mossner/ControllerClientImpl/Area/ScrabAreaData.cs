using Alvasoft.ConnectionHolder;
using log4net;

namespace Alvasoft.Mossner.ControllerClientImpl.Area
{
    /// <summary>
    /// Область обрези.
    /// </summary>
    public class ScrabAreaData : AbstractAreaData2
    {
        private static readonly ILog Logger = LogManager.GetLogger("InputAreaData");

        private const string ITEMID_SCRABAREA_ID_CHECK_OUT = "L3_Interface.OUT.ScrArea_ID_Check";
        private const string ITEMID_SCRABAREA_ID_NUMBER_OUT = "L3_Interface.OUT.ScrArea_ID_Number";
        private const string ITEMID_SCRABAREA_WEIGHT_OUT = "L3_Interface.OUT.ScrArea_Weight";
        private const string ITEMID_SCRABAREA_LENGTH_OUT = "L3_Interface.OUT.ScrArea_Length";
        private const string ITEMID_SCRABAREA_ID_CHECK_IN = "L3_Interface.IN.ScrArea_ID_CheckResult";
        private const string ITEMID_SCRABAREA_ID_NUMBER_IN = "L3_Interface.IN.ScrArea_ID_Number";

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aTopicName"></param>
        /// <param name="aOpcConnection"></param>
        public ScrabAreaData(string aTopicName, OpcConnectionHolder aOpcConnection)
        {
            IdCheckOutputItem = new DataItem(aTopicName + ITEMID_SCRABAREA_ID_CHECK_OUT, aOpcConnection);
            IdNumberOutputItem = new DataItem(aTopicName + ITEMID_SCRABAREA_ID_NUMBER_OUT, aOpcConnection);
            IdCheckInputItem = new DataItem(aTopicName + ITEMID_SCRABAREA_ID_CHECK_IN, aOpcConnection);
            IdNumberInputItem = new DataItem(aTopicName + ITEMID_SCRABAREA_ID_NUMBER_IN, aOpcConnection);
            WeightOutputItem = new DataItem(aTopicName + ITEMID_SCRABAREA_WEIGHT_OUT, aOpcConnection);
            LengthOutputItem = new DataItem(aTopicName + ITEMID_SCRABAREA_LENGTH_OUT, aOpcConnection);
        }

        /// <summary>
        /// Инициализация.
        /// </summary>
        public void Initialize()
        {
            Logger.Info("Инициализация ScrabAreaData...");
            IdCheckOutputItem.Initialize();
            IdNumberOutputItem.Initialize();
            IdCheckInputItem.Initialize();
            IdNumberInputItem.Initialize();
            WeightOutputItem.Initialize();
            LengthOutputItem.Initialize();
            Logger.Info("Инициализация ScrabAreaData завершена.");
        }

        /// <summary>
        /// Деинициализация.
        /// </summary>
        public void Uninitialize()
        {
            Logger.Info("Деинициализация ScrabAreaData...");
            IdCheckOutputItem.Uninitialize();
            IdNumberOutputItem.Uninitialize();
            IdCheckInputItem.Uninitialize();
            IdNumberInputItem.Uninitialize();
            WeightOutputItem.Uninitialize();
            LengthOutputItem.Uninitialize();
            Logger.Info("Деинициализация ScrabAreaData завершена.");
        }
    }
}
