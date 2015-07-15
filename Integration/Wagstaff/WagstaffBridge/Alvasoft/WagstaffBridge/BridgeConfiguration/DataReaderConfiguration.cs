using System;
using System.Collections.Generic;
using System.Xml;

namespace Alvasoft.WagstaffBridge.BridgeConfiguration
{
    /// <summary>
    /// Конфигурация DataReader.
    /// </summary>
    public class DataReaderConfiguration
    {
        private const string NODE_READ_INTERVAL = "ReadInterval";
        private const string NODE_DATA = "Data";
        private const string NODE_ITEM = "Item";
        private const string NODE_OPC_ITEM_NAME = "OpcItemName";
        private const string NODE_OPC_TYPE_NAME = "TypeName";
        private const string NODE_OPC_OBJECT_NAME = "ObjectName";
        private const string NODE_OPC_DATA_NAME = "DataName";

        public List<DataReadInfo> DataInfo { get; set; }
        public double ReadInterval { get; set; }

        /// <summary>
        /// Загрузить конфигурацию из указанного XmlNode'а.
        /// </summary>
        /// <param name="aNode">XmlNode.</param>
        public void LoadFromXmlNode(XmlNode aNode)
        {
            DataInfo = new List<DataReadInfo>();
            var nodes = aNode.ChildNodes;
            for (var fieldIndex = 0; fieldIndex < nodes.Count; ++fieldIndex) {
                switch (nodes[fieldIndex].Name) {
                    case NODE_READ_INTERVAL:
                        ReadInterval = double.Parse(nodes[fieldIndex].InnerText);
                        break;
                    case NODE_DATA:
                        for (var i = 0; i < nodes[fieldIndex].ChildNodes.Count; ++i) {
                            var dataInfo = LoadDataReadInfoFromNode(nodes[fieldIndex].ChildNodes[i]);
                            DataInfo.Add(dataInfo);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Загрузить информацию конкретного DataReadInfo.
        /// </summary>
        /// <param name="aNode">XmlNode</param>
        /// <returns>Загруженный DataReadInfo</returns>
        private DataReadInfo LoadDataReadInfoFromNode(XmlNode aNode)
        {
            var dataInfo = new DataReadInfo();
            var nodes = aNode.ChildNodes;
            for (var fieldIndex = 0; fieldIndex < nodes.Count; ++fieldIndex) {
                switch (nodes[fieldIndex].Name) {
                    case NODE_OPC_ITEM_NAME:
                        dataInfo.OpcItemName = nodes[fieldIndex].InnerText;
                        break;
                    case NODE_OPC_TYPE_NAME:
                        dataInfo.TypeName = nodes[fieldIndex].InnerText;
                        break;
                    case NODE_OPC_OBJECT_NAME:
                        dataInfo.ObjectName = nodes[fieldIndex].InnerText;
                        break;
                    case NODE_OPC_DATA_NAME:
                        dataInfo.DataName = nodes[fieldIndex].InnerText;
                        break;
                }
            }

            return dataInfo;
        }

        /// <summary>
        /// Установить идентификаторы на основе данных из ИТЦ.
        /// </summary>
        /// <param name="aReadDataInfoList"></param>
        public void SetIdsToOpcItems(List<DataReadInfo> aReadDataInfoList)
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
