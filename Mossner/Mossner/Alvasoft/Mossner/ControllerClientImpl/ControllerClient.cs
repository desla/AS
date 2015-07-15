using System;
using Alvasoft.ConnectionHolder;
using Alvasoft.Mossner.ControllerClientImpl.Area;
using Alvasoft.Utils.Activity;
using log4net;

namespace Alvasoft.Mossner.ControllerClientImpl
{
    /// <summary>
    /// Клиент для взаимодействия с OPC-тегами.
    /// </summary>
    public class ControllerClient : InitializableImpl
    {
        private static readonly ILog Logger = LogManager.GetLogger("ControllerClient");

        private InputAreaData inputArea;
        private OutputAreaData outputArea;
        private ScrabAreaData scrabArea;

        private bool isInputAreaFisrtTime = true;
        private bool isOutputAreaFisrtTime = true;
        private bool isScrabAreaFisrtTime = true; 

        private ControllerClientCallback callback;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aTopicName">Имя топика.</param>
        /// <param name="aOpcConnection">Соединение с OPC-сервером.</param>
        public ControllerClient(string aTopicName, OpcConnectionHolder aOpcConnection)
        {
            inputArea = new InputAreaData(aTopicName, aOpcConnection);
            outputArea = new OutputAreaData(aTopicName, aOpcConnection);
            scrabArea = new ScrabAreaData(aTopicName, aOpcConnection);
        }        

        /// <summary>
        /// Устанавливает callback для обратной связи.
        /// </summary>
        /// <param name="aCallback">Callback.</param>
        public void SetCallback(ControllerClientCallback aCallback)
        {
            callback = aCallback;
        }        

        /// <summary>
        /// Входная область: Записывает идентификатор слитка.
        /// </summary>
        /// <param name="aSlabId">Идентификатор слитка.</param>
        public void WriteSlabIdForInputArea(int aSlabId)
        {
            try {
                inputArea.IdNumberInputItem.Write(aSlabId);
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Входная область: Записывает результат.
        /// </summary>
        /// <param name="aResult">Результат. (1-OK; 2-ID not exist; 3=ID exist)</param>
        public void WriteResultForInputArea(int aResult)
        {
            try {
                inputArea.IdCheckInputItem.Write(aResult);
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Область обрези: Записывает идентификатор слитка.
        /// </summary>
        /// <param name="aSlabId">Идентификатор слитка.</param>
        public void WriteSlabIdForScrabArea(int aSlabId)
        {
            try {
                scrabArea.IdNumberInputItem.Write(aSlabId);
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Область обрези: Записывает результат.
        /// </summary>
        /// <param name="aResult">Результат. (1-OK; 2-ID not exist; 3=ID exist)</param>
        public void WriteResultForScrabArea(int aResult)
        {
            try {
                scrabArea.IdCheckInputItem.Write(aResult);
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Выходная область: Записывает идентификатор слитка.
        /// </summary>
        /// <param name="aSlabId">Идентификатор слитка.</param>
        public void WriteSlabIdForOutputArea(int aSlabId)
        {
            try {
                outputArea.IdNumberInputItem.Write(aSlabId);
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Область обрези: Записывает результат.
        /// </summary>
        /// <param name="aResult">Результат. (1-OK; 2-ID not exist; 3=ID exist)</param>
        public void WriteResultForOutputArea(int aResult)
        {
            try {
                outputArea.IdCheckInputItem.Write(aResult);
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Инициализация.
        /// </summary>
        protected override void DoInitialize()
        {
            Logger.Info("Инициализация ControllerClient...");
            inputArea.Initialize();
            outputArea.Initialize();
            scrabArea.Initialize();

            var inputGroup = inputArea.IdCheckOutputItem.Group;            
            inputGroup.UpdateRate = 1000;
            inputGroup.DataChange += InputAreaCheckOfIdNumberChanged;

            var outputGroup = outputArea.IdCheckOutputItem.Group;                                    
            outputGroup.UpdateRate = 1000;
            outputGroup.DataChange += OutputAreaCheckOfIdNumberChanged;

            var scrabGroup = scrabArea.IdCheckOutputItem.Group;                                    
            scrabGroup.UpdateRate = 1000;
            scrabGroup.DataChange += ScrabAreaCheckOfIdNumberChanged;

            inputGroup.IsActive = true;
            outputGroup.IsActive = true;
            scrabGroup.IsActive = true;            

            inputGroup.IsSubscribed = true;            
            outputGroup.IsSubscribed = true;
            scrabGroup.IsSubscribed = true;

            Logger.Info("Инициализация ControllerClient завершена.");
        }

        /// <summary>
        /// Деинициализация.
        /// </summary>
        protected override void DoUninitialize()
        {
            Logger.Info("Деинициализация ControllerClient...");
            inputArea.Uninitialize();
            outputArea.Uninitialize();
            scrabArea.Uninitialize();
            Logger.Info("Деинициализация ControllerClient завершена.");
        }

        /// <summary>
        /// Выходная область: работает при изменеии тега проверки результата.
        /// </summary>
        /// <param name="transactionid">Номер транзации.</param>
        /// <param name="numitems">Количество измененных параметров.</param>
        /// <param name="clienthandles">Клиенты.</param>
        /// <param name="itemvalues">Значения.</param>
        /// <param name="qualities">Качество значений.</param>
        /// <param name="timestamps">Время изменения.</param>
        private void OutputAreaCheckOfIdNumberChanged(
            int transactionid, 
            int numitems, 
            ref Array clienthandles, 
            ref Array itemvalues, 
            ref Array qualities, 
            ref Array timestamps)
        {
            if (isOutputAreaFisrtTime) {
                isOutputAreaFisrtTime = false;
                return;
            }

            try {
                var currentCheckState = Convert.ToInt32(itemvalues.GetValue(1));

                Logger.Debug("Выходная область: Идентификация номера слитка.");
                if (currentCheckState == 0) {
                    Logger.Debug("Выходная область: Сброс.");
                    outputArea.IdCheckInputItem.Write(0);
                    outputArea.IdNumberInputItem.Write(0);
                }
                else {
                    var slabId = Convert.ToInt32(outputArea.IdNumberOutputItem.Read());
                    var slabWeight = Convert.ToInt32(outputArea.WeightOutputItem.Read());
                    var slabLength = Convert.ToInt32(outputArea.LengthOutputItem.Read());
                    Logger.Debug(string.Format("Идентификатор слитка={0}; Вес={1}; Длинна={2}",
                        slabId, slabWeight, slabLength));

                    if (callback != null) {
                        callback.ReadSlabIdCheckForOutputArea(this, slabId, slabWeight, slabLength);
                    }
                }
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Область обрези: работает при измении тега проверки для области обрези.
        /// </summary>
        /// <param name="transactionid">Транзакция.</param>
        /// <param name="numitems">Количество параметров.</param>
        /// <param name="clienthandles">Клиенты.</param>
        /// <param name="itemvalues">Значения.</param>
        /// <param name="qualities">Качество значений.</param>
        /// <param name="timestamps">Время изменения.</param>
        private void ScrabAreaCheckOfIdNumberChanged(
            int transactionid, 
            int numitems, 
            ref Array clienthandles, 
            ref Array itemvalues, 
            ref Array qualities, 
            ref Array timestamps)
        {
            if (isScrabAreaFisrtTime) {
                isScrabAreaFisrtTime = false;
                return;
            }

            try {
                var currentCheckState = Convert.ToInt32(itemvalues.GetValue(1));

                Logger.Debug("Область обрези: Идентификация номера слитка.");

                if (currentCheckState == 0) {
                    Logger.Debug("Область обрези: Сброс.");
                    scrabArea.IdCheckInputItem.Write(0);
                    scrabArea.IdNumberInputItem.Write(0);
                }
                else {
                    // Номер слитка.
                    var slabId = Convert.ToInt32(scrabArea.IdNumberOutputItem.Read());
                    var slabWeight = Convert.ToInt32(scrabArea.WeightOutputItem.Read());
                    var slabLength = Convert.ToInt32(scrabArea.LengthOutputItem.Read());
                    Logger.Debug(string.Format("Идентификатор слитка={0}; Вес={1}; Длинна={2}",
                        slabId, slabWeight, slabLength));
                    if (callback != null) {
                        callback.ReadSlabIdCheckForScrabArea(this, slabId, slabWeight, slabLength);
                    }
                }
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Входная область: работает при изменении тега проверки для входной области.
        /// </summary>
        /// <param name="transactionid">Транзакция.</param>
        /// <param name="numitems">Количество параметров.</param>
        /// <param name="clienthandles">Клиенты.</param>
        /// <param name="itemvalues">Значения.</param>
        /// <param name="qualities">Качество значений.</param>
        /// <param name="timestamps">Время изменения.</param>
        private void InputAreaCheckOfIdNumberChanged(
            int transactionid, 
            int numitems, 
            ref Array clienthandles, 
            ref Array itemvalues, 
            ref Array qualities, 
            ref Array timestamps)
        {
            if (isInputAreaFisrtTime) {
                isInputAreaFisrtTime = false;
                return;
            }

            try {
                var currentCheckState = Convert.ToInt32(itemvalues.GetValue(1));

                Logger.Debug("Входная область: Идентификация номера слитка.");

                if (currentCheckState == 0) {
                    // Контроллер завершил обработку сообщения.
                    // Очищаем ему вход.
                    Logger.Debug("Входная область: Сброс.");
                    inputArea.IdCheckInputItem.Write(0);
                    inputArea.IdNumberInputItem.Write(0);
                }
                else {
                    // Контроллер послал сообщение "Запрос идентификации номера слитка для входной области".
                    // Получаем идентификатор слитка, переданный из контроллера.
                    var slabId = Convert.ToInt32(inputArea.IdNumberOutputItem.Read());
                    Logger.Debug("Идентификатор слитка: " + slabId);
                    if (callback != null) {
                        // Проверяем в ИТС номер слитка.
                        callback.ReadSlabIdCheckForInputArea(this, slabId);
                    }
                }
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }                
    }
}
