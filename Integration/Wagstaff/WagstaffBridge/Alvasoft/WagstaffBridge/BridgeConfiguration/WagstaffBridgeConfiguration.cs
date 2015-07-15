using System;
using System.Collections.Generic;
using System.Xml;
using Alvasoft.Utils.Activity;
using Alvasoft.WagstaffBridge.ConnectionHolder;
using log4net;
using Oracle.ManagedDataAccess.Client;

namespace Alvasoft.WagstaffBridge.BridgeConfiguration
{
    /// <summary>
    /// Общая конфигурация приложения. 
    /// </summary>
    public class WagstaffBridgeConfiguration : InitializableImpl
    {
        private static readonly ILog Logger = LogManager.GetLogger("DataBuffer");

        private const string NODE_WAGSTAFF_CURDATA = "WagstaffCurrdata";
        private const string NODE_MIXER_CURDATA = "MixerCurdata";

        private const string NODE_ORACLE = "ITS_Oracle";
        private const string NODE_WASTAFF_SQL = "Wagstaff_MSSQL";
        private const string NODE_OPC = "OPC";
        private const string NODE_BUFFER_SQL = "Buffer_MSSQL";

        private OracleConnectionHolder oracleConnection;

        public List<DataReadInfo> DataInfo { get; private set; }

        public DataReaderConfiguration WagstaffCurdataConfiguration { get; private set; }
        public DataReaderConfiguration MixerCurrdataConfiguration { get; private set; }

        public ConnectionConfiguration oracleConfiguration;
        public ConnectionConfiguration wagsSqlConfiguration;
        public ConnectionConfiguration opcConfiguration;
        public ConnectionConfiguration bufSqlConfiguration;

        /// <summary>
        /// Установить ConnectionHolder для инициализации конфигурации.
        /// </summary>
        /// <param name="aConnection">ConnectionHolder.</param>
        public void SetOracleConnection(OracleConnectionHolder aConnection)
        {
            oracleConnection = aConnection;
        }

        /// <summary>
        /// Загрузить сетевую конфигурацию из файла XML.
        /// </summary>
        /// <param name="aXmlFile">Путь к файлу конфигурации.</param>
        public void LoadConnectionsConfiguration(string aXmlFile)
        {
            oracleConfiguration = new ConnectionConfiguration();
            wagsSqlConfiguration = new ConnectionConfiguration();
            opcConfiguration = new ConnectionConfiguration();
            bufSqlConfiguration = new ConnectionConfiguration();

            var document = new XmlDocument();
            document.Load(aXmlFile);

            var root = document.DocumentElement;
            var nodes = root.ChildNodes;
            for (var nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex) {
                switch (nodes[nodeIndex].Name) {
                    case NODE_ORACLE:
                        oracleConfiguration.LoadFromXmlNode(nodes[nodeIndex]);
                        break;
                    case NODE_WASTAFF_SQL:
                        wagsSqlConfiguration.LoadFromXmlNode(nodes[nodeIndex]);
                        break;
                    case NODE_OPC:
                        opcConfiguration.LoadFromXmlNode(nodes[nodeIndex]);
                        break;
                    case NODE_BUFFER_SQL:
                        bufSqlConfiguration.LoadFromXmlNode(nodes[nodeIndex]);
                        break;
                }
            }
        }

        /// <summary>
        /// Загрузить конфигурацию OPC-тегов из XML файла.
        /// </summary>
        /// <param name="aXmlFile">Имя файла.</param>
        public void LoadOpcItemsConfiguration(string aXmlFile)
        {
            WagstaffCurdataConfiguration = new DataReaderConfiguration();
            MixerCurrdataConfiguration = new DataReaderConfiguration();

            var document = new XmlDocument();
            document.Load(aXmlFile);

            var root = document.DocumentElement;
            var nodes = root.ChildNodes;
            for (var nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex) {
                switch (nodes[nodeIndex].Name) {
                    case NODE_WAGSTAFF_CURDATA:
                        WagstaffCurdataConfiguration.LoadFromXmlNode(nodes[nodeIndex]);
                        break;
                    case NODE_MIXER_CURDATA:
                        MixerCurrdataConfiguration.LoadFromXmlNode(nodes[nodeIndex]);
                        break;
                }
            }

            DataInfo = new List<DataReadInfo>(WagstaffCurdataConfiguration.DataInfo);
            DataInfo.AddRange(MixerCurrdataConfiguration.DataInfo);
        }

        /// <summary>
        /// Инициализация конфигруации. При появлении подключения к ИТЦ считываем идентификаторы данных и настраиваем их у себя.
        /// </summary>
        protected override void DoInitialize()
        {
            Logger.Info("Инициализация конфигурации...");
            var connection = oracleConnection.GetOracleConnection();
            var readDataInfoList = new List<DataReadInfo>();

            try {
                var commandText = "select t.NAME, o.NAME, d.NAME, t.ID, o.ID, d.ID " +
                                  "from TYPE_INFO t, OBJECT_INFO o, DATA_INFO d " +
                                  "where (d.TYPE_INFO_ID = t.ID) and (o.TYPE_INFO_ID = t.ID)";
                using (var command = new OracleCommand(commandText, connection)) {
                    using (var reader = command.ExecuteReader()) {
                        oracleConnection.UpdateLastOperationTime();
                        while (reader.Read()) {
                            var dataInfo = new DataReadInfo();
                            dataInfo.TypeName = (string)reader.GetValue(0);
                            dataInfo.ObjectName = (string)reader.GetValue(1);
                            dataInfo.DataName = (string)reader.GetValue(2);
                            dataInfo.TypeId = int.Parse(reader.GetValue(3).ToString());
                            dataInfo.ObjectId = int.Parse(reader.GetValue(4).ToString());
                            dataInfo.DataId = int.Parse(reader.GetValue(5).ToString());

                            readDataInfoList.Add(dataInfo);
                        }
                    }
                }
            }
            catch (Exception ex) {
                oracleConnection.ProcessError(ex);
            }

            try {
                SetIdsToDataInfo(readDataInfoList);
                WagstaffCurdataConfiguration.SetIdsToOpcItems(DataInfo);
                MixerCurrdataConfiguration.SetIdsToOpcItems(DataInfo);
                Logger.Info("Инициализация конфигурации прошла успешно.");
            }
            catch (Exception ex) {
                Logger.Info("Ошибка при инициализации конфигурации");
                Logger.Error("Ошибка при инициализации конфигурации: " + ex);
            }
        }

        /// <summary>
        /// Установить идентификаторы на основе данных из ИТЦ.
        /// </summary>
        /// <param name="aReadDataInfoList"></param>
        private void SetIdsToDataInfo(List<DataReadInfo> aReadDataInfoList)
        {
            foreach (var source in DataInfo) {
                bool isFind = false;
                foreach (var item in aReadDataInfoList) {
                    if (source.DataName == item.DataName &&
                        source.TypeName == item.TypeName &&
                        source.ObjectName == item.ObjectName) {
                        isFind = true;
                        source.DataId = item.DataId;
                        source.TypeId = item.TypeId;
                        source.ObjectId = item.ObjectId;
                        break;
                    }
                }

                if (!isFind) {
                    throw new ArgumentException("Не найдено идентификатов для " +
                                                source.TypeName + "/" +
                                                source.ObjectName + "/" +
                                                source.DataName);
                }
            }
        }
    }
}
