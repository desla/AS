using System.Xml;

namespace Alvasoft.BridgeConfiguration
{
    /// <summary>
    /// Сетевая конфигурация. Общая.
    /// </summary>
    public class ConnectionConfiguration
    {
        private const string NODE_HOST = "Host";
        private const string NODE_USER = "User";
        private const string NODE_PASSWORD = "Password";
        private const string NODE_DATABASE = "Database";
        private const string NODE_SERVER_NAME = "Server";
        private const string NODE_RECONNECTION_INTERVAL = "ReconnectionInterval";
        private const string NODE_TOPIC_NAME = "TopicName";

        public string Host { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string ServerName { get; set; }
        public double ReconnectionInterval { get; set; }
        public string TopicName { get; private set; }    

        /// <summary>
        /// Загрузить из XmlNode'а.
        /// </summary>
        /// <param name="aNode">XmlNode.</param>
        public void LoadFromXmlNode(XmlNode aNode)
        {
            var nodes = aNode.ChildNodes;
            for (var fieldIndex = 0; fieldIndex < nodes.Count; ++fieldIndex) {
                switch (nodes[fieldIndex].Name) {
                    case NODE_HOST:
                        Host = nodes[fieldIndex].InnerText;
                        break;
                    case NODE_USER:
                        User = nodes[fieldIndex].InnerText;
                        break;
                    case NODE_PASSWORD:
                        Password = nodes[fieldIndex].InnerText;
                        break;
                    case NODE_DATABASE:
                        Database = nodes[fieldIndex].InnerText;
                        break;
                    case NODE_SERVER_NAME:
                        ServerName = nodes[fieldIndex].InnerText;
                        break;
                    case NODE_RECONNECTION_INTERVAL:
                        ReconnectionInterval = double.Parse(nodes[fieldIndex].InnerText);
                        break;
                    case NODE_TOPIC_NAME:
                        TopicName = nodes[fieldIndex].InnerText;
                        break;
                }
            }
        }
    }    
}
