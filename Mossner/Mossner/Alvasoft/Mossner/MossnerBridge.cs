using System;
using Alvasoft.BridgeConfiguration;
using Alvasoft.ConnectionHolder;
using Alvasoft.Utils.Activity;
using log4net;
using Alvasoft.Mossner.ControllerClientImpl;

namespace Alvasoft.Mossner
{
    /// <summary>
    /// Класс реализует взаимодействие с контроллерами mossner.
    /// </summary>
    public class MossnerBridge : InitializableImpl, ConnectionHolderCallback, ControllerClientCallback
    {
        private static readonly ILog Logger = LogManager.GetLogger("MossnerBridge");
        private const string OPCHOLDER = "OPCHOLDER";
        private const string ORACLEHOLDER = "ORACLEHOLDER";

        private MossnerBridgeConfiguration configuration;

        private OpcConnectionHolder opcConnection;
        private OracleConnectionHolder oracleConnection;

        private Its its;
        private LiveBit liveBit;
        private ControllerClient controllerClient;  

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aConfiguration">Конфигурация.</param>
        public MossnerBridge(MossnerBridgeConfiguration aConfiguration)
        {
            configuration = aConfiguration;
        }        

        /// <summary>
        /// Работает при соединении какого-либо коннектора.
        /// </summary>
        /// <param name="aConnection">Подключившийся коннектор.</param>
        public void OnConnected(ConnectionHolderBase aConnection)
        {
            Logger.Info("Подключен " + aConnection.GetHolderName());

            switch (aConnection.GetHolderName()) {                
                case OPCHOLDER:
                    liveBit.Initialize();
                    controllerClient.Initialize();
                    break;                                    
                case ORACLEHOLDER:                    
                    break;
            }
        }

        /// <summary>
        /// Работает при потери связи какого-либо коннектора.
        /// </summary>
        /// <param name="aConnection">Потерявший соединение коннектор.</param>
        public void OnDisconnected(ConnectionHolderBase aConnection)
        {
            Logger.Info("Отключен " + aConnection.GetHolderName());

            switch (aConnection.GetHolderName()) {                
                case OPCHOLDER:
                    liveBit.Uninitialize();
                    controllerClient.Uninitialize();
                    break;                
                case ORACLEHOLDER:                    
                    break;
            }
        }

        /// <summary>
        /// Работает при возникновении ошибки в коннекторе.
        /// </summary>
        /// <param name="aConnection">Коннектор.</param>
        /// <param name="aError">Ошибка.</param>
        public void OnError(ConnectionHolderBase aConnection, Exception aError)
        {
            Logger.Error(aError);
            aConnection.CloseConnection();
        }

        /// <summary>
        /// Работает при готовности тега проверки для входной области.
        /// </summary>
        /// <param name="aControllerClient">Соединение с контроллерами mossner.</param>
        /// <param name="aSlabId">Идентификатор слитка.</param>
        public void ReadSlabIdCheckForInputArea(ControllerClient aControllerClient, int aSlabId)
        {
            aControllerClient.WriteSlabIdForInputArea(aSlabId);

            // Ищем слиток в ИТС...
            var mixerNumber = IdHelper.MixerNumber(aSlabId);
            var meltNumber = IdHelper.MeltNumber(aSlabId);
            var slabNumber = IdHelper.SlabNumber(aSlabId);

            // Ищем плавку.
            var result = its.CheckMelt(mixerNumber, meltNumber);
            if (result == 1) {
                // Плавка найдена.
                // Ищем слиток.
                result = its.CheckSlab(mixerNumber, meltNumber, slabNumber);
                if (result == 1) {
                    // Слиток не найден.
                    Logger.Debug("Cлиток " + aSlabId + " не существует. Можно создавать.");
                    // Добавляем.
                    its.AddSlab(mixerNumber, meltNumber, slabNumber);
                }
                else {
                    Logger.Debug("Cлиток " + aSlabId + " уже существует.");
                }
            }
            else {
                Logger.Debug("Плавка для слитка " + aSlabId + " не существует.");
            }
            
            aControllerClient.WriteResultForInputArea(result);
        }

        /// <summary>
        /// Работает при готовности тега проверки для области обрези.
        /// </summary>
        /// <param name="aControllerClient">Соединение с контроллерами mossner.</param>
        /// <param name="aSlabId">Идентификтор слитка.</param>
        /// <param name="aSlabWeight">Вес слитка.</param>
        /// <param name="aSlabLength">Длина слитка.</param>
        public void ReadSlabIdCheckForScrabArea(ControllerClient aControllerClient, 
            int aSlabId, int aSlabWeight, int aSlabLength)
        {
            aControllerClient.WriteSlabIdForScrabArea(aSlabId);

            // Ищем слиток в ИТС...
            var mixerNumber = IdHelper.MixerNumber(aSlabId);
            var meltNumber = IdHelper.MeltNumber(aSlabId);
            var slabNumber = IdHelper.SlabNumber(aSlabId);

            // Ищем плавку.
            var result = its.CheckMelt(mixerNumber, meltNumber);
            if (result == 1) {
                // Плавка найдена.
                // Ищем слиток.
                result = its.CheckProd(mixerNumber, meltNumber, slabNumber);
                if (result == 1) {
                    // Слиток не найден.
                    // Добавляем.
                    its.AddProd(mixerNumber, meltNumber, slabNumber, aSlabWeight, aSlabLength);
                    Logger.Debug("Создан слиток=" + aSlabId + ", Вес=" + aSlabWeight + ", Длина=" + aSlabLength);
                }
                else {
                    Logger.Debug("Cлиток " + aSlabId + " уже существует.");
                }
            }
            else {
                Logger.Debug("Плавка для слитка " + aSlabId + " не существует.");
            }

            // Для контроллера всегда возвращаем 3!
            result = 3;

            aControllerClient.WriteResultForScrabArea(result);
        }

        /// <summary>
        /// Работает при готовности тега проверки для выходной области.
        /// </summary>
        /// <param name="aControllerClient">Соединение с контроллерами mossner.</param>
        /// <param name="aSlabId">Идентификатор слитка.</param>
        /// <param name="aSlabWeight">Вес слитка.</param>
        /// <param name="aSlabLength">Длина слитка.</param>
        public void ReadSlabIdCheckForOutputArea(ControllerClient aControllerClient, 
            int aSlabId, int aSlabWeight, int aSlabLength)
        {
            aControllerClient.WriteSlabIdForOutputArea(aSlabId);

            // Ищем слиток в ИТС...
            var mixerNumber = IdHelper.MixerNumber(aSlabId);
            var meltNumber = IdHelper.MeltNumber(aSlabId);
            var slabNumber = IdHelper.SlabNumber(aSlabId);
            // Ищем плавку.
            var result = its.CheckMelt(mixerNumber, meltNumber);
            if (result == 1) {
                // Плавка найдена.
                // Ищем слиток.
                result = its.CheckProd(mixerNumber, meltNumber, slabNumber);
                if (result == 1) {
                    // Слиток не найден.
                    // Добавляем.
                    its.AddProd(mixerNumber, meltNumber, slabNumber, aSlabWeight, aSlabLength);
                    Logger.Debug("Создан слиток=" + aSlabId + ", Вес=" + aSlabWeight + ", Длина=" + aSlabLength);
                }
                else {
                    Logger.Debug("Cлиток " + aSlabId + " уже существует.");
                }
            }
            else {
                Logger.Debug("Плавка для слитка " + aSlabId + " не существует.");
            }
            // Для контроллера всегда возвращаем 3!
            result = 3;

            aControllerClient.WriteResultForOutputArea(result);
        }

        /// <summary>
        /// Инициализация.
        /// </summary>
        protected override void DoInitialize()
        {
            try {
                Logger.Info("Инициализация MossnerBridge...");

                oracleConnection = new OracleConnectionHolder(configuration.OracleConfiguration);
                oracleConnection.SetHolderName(ORACLEHOLDER);
                oracleConnection.SetCallback(this);

                opcConnection = new OpcConnectionHolder(configuration.OpcConfiguration);
                opcConnection.SetHolderName(OPCHOLDER);
                opcConnection.SetCallback(this);                

                its = new Its();
                its.SetOracleHolder(oracleConnection);

                liveBit = new LiveBit(configuration.OpcConfiguration.TopicName);
                liveBit.SetOpcConnection(opcConnection);

                controllerClient = new ControllerClient(configuration.OpcConfiguration.TopicName, opcConnection);
                controllerClient.SetCallback(this);

                oracleConnection.OpenConnection();
                opcConnection.OpenConnection();                
                Logger.Info("Инициализация MossnerBridge завершена.");
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Деинициализация.
        /// </summary>
        protected override void DoUninitialize()
        {
            try {
                Logger.Info("Деинициализация MossnerBridge...");
                opcConnection.CloseConnection();
                oracleConnection.CloseConnection();                
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
            finally {
                Logger.Info("Деинициализация MossnerBridge завершена.");
            }
        }
    }
}
