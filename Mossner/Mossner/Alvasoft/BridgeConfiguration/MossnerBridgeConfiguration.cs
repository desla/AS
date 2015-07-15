using System;
using System.Xml;
using log4net;

namespace Alvasoft.BridgeConfiguration
{
    /// <summary>
    /// Общая конфигурация приложения. 
    /// </summary>
    public class MossnerBridgeConfiguration
    {
        private static readonly ILog Logger = LogManager.GetLogger("MossnerBridgeConfiguration");
        
        private const string NODE_ORACLE = "ITS_Oracle";
        private const string NODE_OPC = "OPC";

        public ConnectionConfiguration OracleConfiguration { get; private set; }
        public ConnectionConfiguration OpcConfiguration { get; private set; }        
        
        /// <summary>
        /// Загрузить сетевую конфигурацию из файла XML.
        /// </summary>
        /// <param name="aXmlFile">Путь к файлу конфигурации.</param>
        public void LoadConnectionsConfiguration(string aXmlFile)
        {
            Logger.Info("Загрузка сетевой конфигурации...");
            try {
                OracleConfiguration = new ConnectionConfiguration();
                OpcConfiguration = new ConnectionConfiguration();

                var document = new XmlDocument();
                document.Load(aXmlFile);

                var root = document.DocumentElement;
                var nodes = root.ChildNodes;
                for (var nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex) {
                    switch (nodes[nodeIndex].Name) {
                        case NODE_ORACLE:
                            OracleConfiguration.LoadFromXmlNode(nodes[nodeIndex]);
                            break;
                        case NODE_OPC:
                            OpcConfiguration.LoadFromXmlNode(nodes[nodeIndex]);
                            break;
                    }
                }
            }
            catch (Exception ex) {
                Logger.Error("При загрузке сетевой конфигурации ошибка: " + ex.Message);
            }
        }
    }
}
