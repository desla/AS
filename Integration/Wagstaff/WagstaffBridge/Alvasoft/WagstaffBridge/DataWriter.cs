using System;
using Alvasoft.WagstaffBridge.ConnectionHolder;
using log4net;
using Oracle.ManagedDataAccess.Client;

namespace Alvasoft.WagstaffBridge
{
    /// <summary>
    /// Записывает данные в таблицы ИТЦ.
    /// </summary>
    public class DataWriter
    {
        private static readonly ILog Logger = LogManager.GetLogger("DataWriter");

        private OracleConnectionHolder oracleConnection;
        private OracleCommand insertDataValue;

        /// <summary>
        /// Конструктор. 
        /// </summary>
        public DataWriter()
        {
            BuildCommands();
        }

        /// <summary>
        /// Установить ConnectionHolder для ИТЦ.
        /// </summary>
        /// <param name="aConnectionHolder">ConnectionHolder.</param>
        public void SetOracleConnectionHolder(OracleConnectionHolder aConnectionHolder)
        {
            oracleConnection = aConnectionHolder;
        }

        /// <summary>
        /// Записать данные в базу.
        /// </summary>
        /// <param name="aData">Данные.</param>
        /// <returns>True - если запись прошла успешно, false - иначе.</returns>
        public bool TryWriteData(DataValue aData)
        {
            try {
                oracleConnection.LockConnection();

                Logger.Debug("Попытка записи данных");                

                if (!oracleConnection.IsConnected()) {
                    Logger.Info("Нет подключения к серверу");
                    return false;
                }
                
                insertDataValue.Connection = oracleConnection.GetOracleConnection();
                insertDataValue.Parameters["ObjectId"].Value = aData.ObjectId;
                insertDataValue.Parameters["DataId"].Value = aData.DataId;
                insertDataValue.Parameters["Value"].Value = aData.Value;
                insertDataValue.Parameters["ValueTime"].Value = aData.ValueTime;
                        
                insertDataValue.ExecuteNonQuery();
                oracleConnection.UpdateLastOperationTime();

                Logger.Debug("Запись выполнена успешно");

                return true;
            }
            catch (Exception ex) {
                oracleConnection.ProcessError(ex);
                return false;
            }
            finally {
                oracleConnection.ReleaseConnection();
            }
        }        

        /// <summary>
        /// Постороить sql команды, используемые в DataWriter'е.
        /// </summary>
        private void BuildCommands()
        {
            insertDataValue = new OracleCommand {
                CommandText = "insert into DATA_VALUE(object_info_id, data_info_id, value, value_time) " +
                              "values(:ObjectId, :DataId, :Value, :ValueTime)"
            };
            insertDataValue.Parameters.Add("ObjectId", OracleDbType.Int32);
            insertDataValue.Parameters.Add("DataId", OracleDbType.Int32);
            insertDataValue.Parameters.Add("Value", OracleDbType.Double);
            insertDataValue.Parameters.Add("ValueTime", OracleDbType.TimeStamp);
        }
    }
}
