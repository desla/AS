using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Timers;
using Alvasoft.WagstaffBridge.BridgeConfiguration;
using Alvasoft.WagstaffBridge.ConnectionHolder;
using log4net;

namespace Alvasoft.WagstaffBridge
{
    /// <summary>
    /// Буфер для данных из OPC. В случае отсутствия соединения с ИТЦ сохраняет данные в собственную базу.
    /// При восстановлении связи перебрасывает данные в ИТЦ.
    /// </summary>
    public class DataBuffer
    {
        private static readonly ILog Logger = LogManager.GetLogger("DataBuffer");

        private MsSqlConnectionHolder sqlConnection;
        private SqlCommand selectGetData;
        private SqlCommand insertStoreData;
        private SqlCommand deleteData;

        private DataWriter dataWriter;

        private Timer attemptSendingTimer = new Timer();

        private WagstaffBridgeConfiguration configuration;

        /// <summary>
        /// Буфер для накопления данных в случае отсутствия связи с ИТЦ.
        /// </summary>
        public DataBuffer()
        {
            attemptSendingTimer.Elapsed += TrySendData;
            BuildCommands();
        }        

        /// <summary>
        /// Установить конфигурацию. Так как буфера не важно какие теги сохранять, ему нужна вся конфигурация системы.
        /// </summary>
        /// <param name="aConfiguration">Конфигурация.</param>
        public void SetConfiguration(WagstaffBridgeConfiguration aConfiguration)
        {
            configuration = aConfiguration;
        }

        /// <summary>
        /// Установить состояние.
        /// </summary>
        /// <param name="isActive">True - если запустить работу, false - остановить.</param>
        public void SetActive(bool isActive)
        {
            if (isActive) {
                attemptSendingTimer.Start();
                Logger.Debug("Активирован");
            }
            else {
                attemptSendingTimer.Stop();
                Logger.Debug("Деактивирован");
            }
        }

        /// <summary>
        /// Установить интервал для попыток передачи данных в ИТЦ.
        /// </summary>
        /// <param name="aInterval">Интервал в миллисекундах.</param>
        public void SetAttemptsInterval(double aInterval)
        {
            attemptSendingTimer.Interval = aInterval;
        }

        /// <summary>
        /// Установить ConnectionHolder на буферную таблицу.
        /// </summary>
        /// <param name="aSqlConnection">ConnectionHolder.</param>
        public void SetSqlConnection(MsSqlConnectionHolder aSqlConnection)
        {
            sqlConnection = aSqlConnection;
        }

        /// <summary>
        /// Установить DataWriter, в который производить попытки пересылки данных.
        /// </summary>
        /// <param name="aWriter">DataWriter.</param>
        public void SetDataWriter(DataWriter aWriter)
        {
            dataWriter = aWriter;
        }

        /// <summary>
        /// Попытаться переслать данные из буфера в указанный DataWriter. Метод вызывается таймером.
        /// </summary>
        /// <param name="sender">Object.</param>
        /// <param name="e">ElapsedEventArgs.</param>
        private void TrySendData(object sender = null, ElapsedEventArgs e = null)
        {
            if (!configuration.IsInitialized()) {
                Logger.Debug("Конфигурация не инициализирована");
                return;
            }

            var sendedDataValue = new List<DataValue>();

            try {
                sqlConnection.LockConnection();                

                if (!sqlConnection.IsConnected()) {
                    Logger.Info("Нет подключения к серверу");
                    return;
                }
                
                selectGetData.Connection = sqlConnection.GetSqlConnection();                
                // вытаскиваем все данные из таблицы                                
                using (var reader = selectGetData.ExecuteReader()) {
                    sqlConnection.UpdateLastOperationTime();
                    while (reader.Read()) {
                        var dataValue = new DataValue {
                            Id = reader.GetInt32(0),
                            TypeId = reader.GetInt32(1),
                            ObjectId = reader.GetInt32(2),
                            DataId = reader.GetInt32(3),
                            ValueTime = reader.GetDateTime(4),
                            Value = reader.GetDouble(5),
                            TypeName = reader.GetString(6),
                            ObjectName = reader.GetString(7),
                            DataName = reader.GetString(8)
                        };

                        Logger.Debug("Получены данные из буфера");

                        SetIdsToDataValue(dataValue);                            

                        if (dataWriter.TryWriteData(dataValue)) {
                            Logger.Info("Данные успешно переданы из буфера в ИТС");
                            sendedDataValue.Add(dataValue);
                        }                            
                    }
                }                 
            }
            catch (Exception ex) {
                sqlConnection.ProcessError(ex);
            }
            finally {
                sqlConnection.ReleaseConnection();
            }

            foreach (var dataValue in sendedDataValue) {
                RemoveData(dataValue);
            }

        }

        /// <summary>
        /// Установить данным из буфера все ID по конфигурации
        /// </summary>
        /// <param name="aData"></param>
        private void SetIdsToDataValue(DataValue aData)
        {
            var dataInfoList = configuration.DataInfo;
            foreach (var item in dataInfoList) {
                if (item.TypeName == aData.TypeName && 
                    item.ObjectName == aData.ObjectName &&
                    item.DataName == aData.DataName) {
                    aData.TypeId = item.TypeId;
                    aData.ObjectId = item.ObjectId;
                    aData.DataId = item.DataId;
                    return;
                }
            }

            throw new ArgumentException("В конфигурации нет идентификаторов для данных, полученных из буфера");
        }

        /// <summary>
        /// Установить данным имена по конфигурации.
        /// </summary>
        /// <param name="aData"></param>
        private void SetNamesToDataValue(DataValue aData)
        {
            var dataInfoList = configuration.DataInfo;
            foreach (var item in dataInfoList) {
                if (item.TypeId == aData.TypeId &&
                    item.ObjectId == aData.ObjectId &&
                    item.DataId == aData.DataId) {
                    aData.TypeName = item.TypeName;
                    aData.ObjectName = item.ObjectName;
                    aData.DataName = item.DataName;
                    return;
                }
            }

            throw new ArgumentException("В конфигурации нет идентификаторов для данных, вставляемых в буфер");
        }

        /// <summary>
        /// Сохранить данные в буферную таблицу.
        /// </summary>
        /// <param name="aData">Данные.</param>
        public void StoreData(DataValue aData)
        {
            try {                
                sqlConnection.LockConnection();
                Logger.Debug("Сохранение данных в буфер");
                SetNamesToDataValue(aData);                
                
                insertStoreData.Connection = sqlConnection.GetSqlConnection();
                // сохраняем данные в таблицу - буфер
                insertStoreData.Parameters["TypeId"].Value = -1;
                insertStoreData.Parameters["ObjectId"].Value = -1;
                insertStoreData.Parameters["DataId"].Value = -1;
                insertStoreData.Parameters["ValueTime"].Value = aData.ValueTime;
                insertStoreData.Parameters["Value"].Value = aData.Value;
                insertStoreData.Parameters["TypeName"].Value = aData.TypeName;
                insertStoreData.Parameters["ObjectName"].Value = aData.ObjectName;
                insertStoreData.Parameters["DataName"].Value = aData.DataName;                                
                
                insertStoreData.ExecuteNonQuery();
                sqlConnection.UpdateLastOperationTime();                

                Logger.Debug("Данные успешно сохранены");
            }
            catch (Exception ex) {
                sqlConnection.ProcessError(ex);
            }
            finally {
                sqlConnection.ReleaseConnection();
            }
        }        

        /// <summary>
        /// Удалить данные из буфера.
        /// </summary>
        /// <param name="aData">Данные.</param>
        private void RemoveData(DataValue aData)
        {
            try {                
                sqlConnection.LockConnection();
                Logger.Debug("Удаление данных из буфера");

                deleteData.Connection = sqlConnection.GetSqlConnection();                
                deleteData.Parameters["Id"].Value = aData.Id;                
                
                deleteData.ExecuteNonQuery();
                sqlConnection.UpdateLastOperationTime();                

                Logger.Debug("Данные успешно удалены");
            }
            catch (Exception ex) {
                sqlConnection.ProcessError(ex);
            }
            finally {
                sqlConnection.ReleaseConnection();
            }
        }

        /// <summary>
        /// Постояить все sql команды, используемые в DataBuffer.
        /// </summary>
        private void BuildCommands()
        {
            selectGetData = new SqlCommand {
                CommandText = "select id, type_id, object_id, data_id, value_time, value, " +
                              "       type_name, object_name, data_name " +
                              "from opcBufferTable"
            };

            insertStoreData = new SqlCommand {
                CommandText = "insert into opcBufferTable(type_id, " +
                              "                           object_id, " +
                              "                           data_id, " +
                              "                           value_time, " +
                              "                           value, " +
                              "                           type_name, " +
                              "                           object_name, " +
                              "                           data_name) " +
                              "values(@TypeId, @ObjectId, @DataId, @ValueTime, @Value, @TypeName, @ObjectName, @DataName)"    
            };
            insertStoreData.Parameters.Add("TypeId", SqlDbType.Int);
            insertStoreData.Parameters.Add("ObjectId", SqlDbType.Int);
            insertStoreData.Parameters.Add("DataId", SqlDbType.Int);
            insertStoreData.Parameters.Add("ValueTime", SqlDbType.DateTime);
            insertStoreData.Parameters.Add("Value", SqlDbType.Float);
            insertStoreData.Parameters.Add("TypeName", SqlDbType.VarChar);
            insertStoreData.Parameters.Add("ObjectName", SqlDbType.VarChar);
            insertStoreData.Parameters.Add("DataName", SqlDbType.VarChar);

            deleteData = new SqlCommand {
                CommandText = "delete from opcBufferTable where id = @Id"
            };
            deleteData.Parameters.Add("Id", SqlDbType.Int);
        }
    }
}
