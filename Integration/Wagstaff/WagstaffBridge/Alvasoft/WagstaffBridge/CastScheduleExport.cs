using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Timers;
using Alvasoft.WagstaffBridge.ConnectionHolder;
using log4net;
using Oracle.ManagedDataAccess.Client;

namespace Alvasoft.WagstaffBridge
{
    /// <summary>
    /// Экспортер данных из ИТЦ в Wagstaff.
    /// </summary>
    public class CastScheduleExport
    {
        private static readonly ILog Logger = LogManager.GetLogger("CastScheduleExport");

        private OracleConnectionHolder oracleConnection;
        private MsSqlConnectionHolder sqlConnection;

        private OracleCommand selectCheckNewCast;
        private OracleCommand updateCastState;
        private SqlCommand insertNewCast;

        private Timer checkingTimer = new Timer();

        /// <summary>
        /// Конструктор.
        /// </summary>
        public CastScheduleExport()
        {
            checkingTimer.Elapsed += CheckNewCastSchedule;
            checkingTimer.Enabled = false;

            BuildCommands();
        }        

        /// <summary>
        /// Установить состояние работы.
        /// </summary>
        /// <param name="aActive">True - запустить, false - остановить.</param>
        public void SetActive(bool aActive)
        {
            if (aActive) {
                checkingTimer.Start();
                Logger.Debug("Активирован");
            }
            else {
                checkingTimer.Stop();
                Logger.Debug("Деактивирован");
            }
        }

        /// <summary>
        /// Установить интервал сканирования таблицы на наличие новых записей.
        /// </summary>
        /// <param name="aCheckingInterval"></param>
        public void SetCheckingInterval(double aCheckingInterval)
        {
            checkingTimer.Interval = aCheckingInterval;
        }

        /// <summary>
        /// Установить ConnectionHolder на ИТЦ.
        /// </summary>
        /// <param name="aConnection"></param>
        public void SetOracleConnection(OracleConnectionHolder aConnection)
        {
            oracleConnection = aConnection;
        }

        /// <summary>
        /// Установить ConnectionHolder на Wagstaff.
        /// </summary>
        /// <param name="aConnection"></param>
        public void SetSqlConnection(MsSqlConnectionHolder aConnection)
        {
            sqlConnection = aConnection;
        }

        /// <summary>
        /// Проверить новые записи и, если они есть, переслать в Wagstaff.
        /// Метод запускается таймером.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void CheckNewCastSchedule(object sender = null, ElapsedEventArgs e = null)
        {
            var casts = new List<CastSchedule>();
            var results = new List<ScheduleState>();
            try {
                oracleConnection.LockConnection();
                Logger.Debug("Проверка новых плавок...");

                selectCheckNewCast.Connection = oracleConnection.GetOracleConnection();
                selectCheckNewCast.Parameters["State"].Value = (int) ScheduleState.NOT_PROCESSED;                
                // делаем выборку записей из ITS                                
                using (var reader = selectCheckNewCast.ExecuteReader()) {
                    oracleConnection.UpdateLastOperationTime();
                    while (reader.Read()) {
                        var castSchedule = new CastSchedule {
                            Id = reader.GetInt32(0),
                            RecipeName = reader.GetString(1),
                            RecipeRevision = reader.GetInt32(2),
                            CastNumber = reader.GetString(3),
                            ScheduleState = reader.GetInt32(4),
                            Priority = reader.GetInt32(5)
                        };

                        Logger.Debug("Получены новые плавки");

                        var isSaved = TrySaveToWagstaff(castSchedule);
                        casts.Add(castSchedule);
                        results.Add(isSaved ? ScheduleState.WAS_SAVED : ScheduleState.WAS_ERROR);
                    }
                }                
            }
            catch (Exception ex) {
                oracleConnection.ProcessError(ex);
            }
            finally {
                oracleConnection.ReleaseConnection();
            }

            for (var i = 0; i < casts.Count; ++i) {
                MarkCastScheduleAsSended(casts[i], results[i]);
            }
        }

        /// <summary>
        /// Пометить запись в ITS как отправленную, либо не отправленную (ошибка).
        /// </summary>
        /// <param name="aCastSchedule">Запись.</param>
        /// <param name="aState">Состояние отправки.</param>
        private void MarkCastScheduleAsSended(CastSchedule aCastSchedule, ScheduleState aState)
        {
            try {
                oracleConnection.LockConnection();
                Logger.Debug(string.Format("Изменение состояния записи на '{0}'", aState));

                updateCastState.Connection = oracleConnection.GetOracleConnection();
                updateCastState.Parameters["State"].Value = (int) aState;
                updateCastState.Parameters["Id"].Value = aCastSchedule.Id;                

                updateCastState.ExecuteNonQuery();
                oracleConnection.UpdateLastOperationTime();                

                Logger.Debug("Изменение состояния успешно");
            }
            catch (Exception ex) {
                oracleConnection.ProcessError(ex);
            }
            finally {
                oracleConnection.ReleaseConnection();
            }
        }

        /// <summary>
        /// Сохраняет запись в Wagstaff.
        /// </summary>
        /// <param name="aCastSchedule">Запись.</param>
        /// <returns>True - если запись успешно сохранена, False - иначе.</returns>
        private bool TrySaveToWagstaff(CastSchedule aCastSchedule)
        {
            try {                
                sqlConnection.LockConnection();
                Logger.Debug("Попытка передачи данных в Wagstaff");
                insertNewCast.Connection = sqlConnection.GetSqlConnection();                                
                insertNewCast.Parameters["CastId"].Value = aCastSchedule.CastNumber;
                insertNewCast.Parameters["RecipeId"].Value = aCastSchedule.RecipeName;
                insertNewCast.Parameters["RecipeRev"].Value = aCastSchedule.RecipeRevision;
                insertNewCast.Parameters["Priority"].Value = aCastSchedule.Priority;
                insertNewCast.Parameters["Archived"].Value = 0;
                insertNewCast.Parameters["Node"].Value = "WAGTS1";
                insertNewCast.Parameters["DateCreated"].Value = DateTime.Now;

                insertNewCast.ExecuteNonQuery();
                sqlConnection.UpdateLastOperationTime();
                
                Logger.Debug("Данные переданы успешно");
                
                return true;
            }
            catch (Exception ex) {
                sqlConnection.ProcessError(ex);
                return false;
            }
            finally {
                sqlConnection.ReleaseConnection();
            }
        }

        /// <summary>
        /// Построить sql команды, используемые в текущем классе.
        /// </summary>
        private void BuildCommands()
        {
            selectCheckNewCast = new OracleCommand {
                CommandText = "select a.ID, b.NAME, b.REVISION, " +
                              "       a.CAST_NUMBER, a.SCHEDULE_STATE, a.PRIORITY " +
                              "from CAST_SCHEDULE a, RECIPE_INFO b " +
                              "where (a.RECIPE_ID=b.ID) and (a.SCHEDULE_STATE=:State)"
            };
            selectCheckNewCast.Parameters.Add("State", OracleDbType.Int32);

            updateCastState = new OracleCommand {
                CommandText = "update CAST_SCHEDULE " +
                              "set SCHEDULE_STATE=:State " +
                              "where id=:Id"
            };
            updateCastState.Parameters.Add("State", OracleDbType.Int32);
            updateCastState.Parameters.Add("Id", OracleDbType.Int32);

            insertNewCast = new SqlCommand {
                CommandText = "insert into tblCastSchedule(castId, recipeId, recipeRev, priority, archived, node, dateCreated) " +
                              "values(@CastId, @RecipeId, @RecipeRev, @Priority, @Archived, @Node, @DateCreated)"
            };
            insertNewCast.Parameters.Add("CastId", SqlDbType.VarChar);
            insertNewCast.Parameters.Add("RecipeId", SqlDbType.VarChar);
            insertNewCast.Parameters.Add("RecipeRev", SqlDbType.Int);
            insertNewCast.Parameters.Add("Priority", SqlDbType.Int);
            insertNewCast.Parameters.Add("Archived", SqlDbType.Int);
            insertNewCast.Parameters.Add("Node", SqlDbType.VarChar);
            insertNewCast.Parameters.Add("DateCreated", SqlDbType.DateTime);
        }
    }
}
