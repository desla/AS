using System;
using System.Windows.Forms;
using Alvasoft.WagstaffBridge.BridgeConfiguration;
using Alvasoft.WagstaffBridge.ConnectionHolder;
using Alvasoft.WagstaffBridge.OpcDataReader;
using log4net;
using Alvasoft.Utils.Activity;

namespace Alvasoft.WagstaffBridge
{
    /// <summary>
    /// Связывает все элементы системы.
    /// </summary>
    public class WagstaffBridge : InitializableImpl, ConnectionHolderCallback, DataReaderCallback
    {
        private static readonly ILog Logger = LogManager.GetLogger("WagstaffBridge");

        private WagstaffBridgeConfiguration configuration = new WagstaffBridgeConfiguration();

        private const string OPCHOLDER = "OPCHOLDER";
        private const string ORACLEHOLDER = "ORACLEHOLDER";
        private const string WAGSSQLHOLDER = "WAGSSQLHOLDER";
        private const string BUFSQLHOLDER = "BUFSQLHOLDER";

        private OpcConnectionHolder opcConnection;
        private OracleConnectionHolder oracleConnection;
        private MsSqlConnectionHolder wagsSqlConnection;
        private MsSqlConnectionHolder bufferSqlConnection;

        private DataReaderActivator wagstaffActivator;
        private DataReader wagstaffCurrdata;
        private DataReader mixerCurrData;        
        private DataWriter its;         

        private DataBuffer buffer;

        private CastScheduleExport castScheduleExport;
        private RecipeImport recipeImport;

        /// <summary>
        /// Инициализация и настройска элементов системы.
        /// </summary>
        protected override void DoInitialize()
        {
            try {
                LoadConfiguration();

                Logger.Info("Инициализация...");                                
                opcConnection = new OpcConnectionHolder(configuration.opcConfiguration);
                opcConnection.SetHolderName(OPCHOLDER);
                opcConnection.SetCallback(this);
                oracleConnection = new OracleConnectionHolder(configuration.oracleConfiguration);
                oracleConnection.SetHolderName(ORACLEHOLDER);
                oracleConnection.SetCallback(this);
                wagsSqlConnection = new MsSqlConnectionHolder(configuration.wagsSqlConfiguration);
                wagsSqlConnection.SetHolderName(WAGSSQLHOLDER);
                wagsSqlConnection.SetCallback(this);
                bufferSqlConnection = new MsSqlConnectionHolder(configuration.bufSqlConfiguration);
                bufferSqlConnection.SetHolderName(BUFSQLHOLDER);
                bufferSqlConnection.SetCallback(this);

                wagstaffCurrdata = new DataReader();
                wagstaffCurrdata.SetCallback(this);
                wagstaffCurrdata.SetConfiguration(configuration.WagstaffCurdataConfiguration);                
                wagstaffCurrdata.SetOpcConnectionHolder(opcConnection);

                wagstaffActivator = new DataReaderActivator();
                wagstaffActivator.SetOpcConnection(opcConnection);
                wagstaffActivator.SetDataReader(wagstaffCurrdata);
                wagstaffActivator.SetActivationItemName("[WAGT0]SYS_PV_StatePos");

                mixerCurrData = new DataReader();
                mixerCurrData.SetCallback(this);
                mixerCurrData.SetConfiguration(configuration.MixerCurrdataConfiguration);
                mixerCurrData.SetOpcConnectionHolder(opcConnection);

                its = new DataWriter();
                its.SetOracleConnectionHolder(oracleConnection);

                buffer = new DataBuffer();
                configuration.SetOracleConnection(oracleConnection); // для инициализации
                buffer.SetConfiguration(configuration);
                buffer.SetSqlConnection(bufferSqlConnection);
                buffer.SetDataWriter(its);
                buffer.SetAttemptsInterval(5 * 60 * 1000); // в миллисекундах

                castScheduleExport = new CastScheduleExport();
                castScheduleExport.SetCheckingInterval(60 * 1000);                
                castScheduleExport.SetOracleConnection(oracleConnection);
                castScheduleExport.SetSqlConnection(wagsSqlConnection);

                recipeImport = new RecipeImport();
                recipeImport.SetCheckingInterval(60 * 1000);                
                recipeImport.SetOracleConnection(oracleConnection);
                recipeImport.SetSqlConnection(wagsSqlConnection);

                opcConnection.OpenConnection();
                oracleConnection.OpenConnection();
                bufferSqlConnection.OpenConnection();
                wagsSqlConnection.OpenConnection();
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Загрузка конфигурации из конфигурационных файлов.
        /// </summary>
        private void LoadConfiguration()
        {
            var appPath = Application.StartupPath + "\\";
            configuration.LoadOpcItemsConfiguration(appPath + "Configuration\\OpcItems.xml");
            configuration.LoadConnectionsConfiguration(appPath + "Configuration\\Network.xml");                       
        }

        /// <summary>
        /// Деинициализация. Закрытие соединений.
        /// </summary>
        protected override void DoUninitialize()
        {
            try {
                Logger.Info("Деинициализация...");
                opcConnection.CloseConnection();
                oracleConnection.CloseConnection();
                wagsSqlConnection.CloseConnection();
                bufferSqlConnection.CloseConnection();
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
            finally {
                Logger.Info("Деинициализация произведена.");
            }
        }

        /// <summary>
        /// Обратная связь. Происходит при подключении какого-либо ConnectionHolder'а, на который подписан WagstaffBridge.
        /// </summary>
        /// <param name="aConnection">ConnectionHolder, который успешно подключился.</param>
        public void OnConnected(ConnectionHolderBase aConnection)
        {
            Logger.Info("Подключен " + aConnection.GetHolderName());

            switch (aConnection.GetHolderName()) {
                case BUFSQLHOLDER:                    
                    buffer.SetActive(true);
                    break;
                case OPCHOLDER:                    
                    wagstaffActivator.Initialize();                    
                    mixerCurrData.Initialize();                    
                    mixerCurrData.SetActive(true);
                    break;
                case WAGSSQLHOLDER:
                    recipeImport.SetActive(true);
                    break;
                case ORACLEHOLDER:
                    if (!configuration.IsInitialized()) {
                        configuration.Initialize();
                    }
                    castScheduleExport.SetActive(true);
                    break;
            }            
        }

        /// <summary>
        /// Обратная связь. Происходит при отключении ConnectionHolder'а.
        /// </summary>
        /// <param name="aConnection">ConnectionHolder, который потерял связь.</param>
        public void OnDisconnected(ConnectionHolderBase aConnection)
        {
            Logger.Info("Отключен " + aConnection.GetHolderName());

            switch (aConnection.GetHolderName()) {
                case BUFSQLHOLDER:                    
                    buffer.SetActive(false);
                    break;
                case OPCHOLDER:                    
                    wagstaffActivator.Uninitialize();
                    mixerCurrData.Uninitialize();                               
                    mixerCurrData.SetActive(false);
                    break;
                case WAGSSQLHOLDER:
                    recipeImport.SetActive(false);
                    break;
                case ORACLEHOLDER:
                    castScheduleExport.SetActive(false);
                    break;
            }
        }

        /// <summary>
        /// Обратная связь. Происходит при ошибке ConnectionHolder'а.
        /// </summary>
        /// <param name="aConnection">ConnectionHolder, в котором призошла ошибка.</param>
        /// <param name="aError">Ошибка.</param>
        public void OnError(ConnectionHolderBase aConnection, Exception aError)
        {
            Logger.Error(aError);
            aConnection.CloseConnection();
        }

        /// <summary>
        /// Обратная связь. Происходит при получении данных из DataReader'а, на который подписан текущий WagstaffBridge.
        /// </summary>
        /// <param name="aDataValue">Прочитанные данные.</param>
        public void OnReadedData(DataValue aDataValue)
        {
            if (its.TryWriteData(aDataValue)) {
                Logger.Info("Данные успешно сохранены в ITS");
            }
            else {
                Logger.Info("Данные сохранены в буферную таблицу");
                buffer.StoreData(aDataValue);
            }            
        }
    }
}
