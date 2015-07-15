using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Timers;
using Alvasoft.WagstaffBridge.ConnectionHolder;
using log4net;
using Oracle.ManagedDataAccess.Client;

namespace Alvasoft.WagstaffBridge
{
    /// <summary>
    /// Импортер рецептов из Wagstaff в ИТЦ
    /// </summary>
    public class RecipeImport
    {
        private static readonly ILog Logger = LogManager.GetLogger("RecipeImport");

        private MsSqlConnectionHolder sqlConnection;
        private OracleConnectionHolder oracleConnection;

        // для хранения времени последнего импортированного рецепта
        private const string fileName = "Configuration//LastImportedTime.config";
        private Timer checkingTimer = new Timer();
        private DateTime lastImportedTime;

        private SqlCommand selectGetNewRicipes;
        private SqlCommand selectGetRecipesUpdateArcived;
        private OracleCommand updateRecipeArchived;
        private OracleCommand insertImportNewRecipe;

        /// <summary>
        /// Конструктор импортера рецептов. Настраивает дату последнего рецепта.
        /// Если не удалось прочитать, устанавливает минимальную.
        /// </summary>
        public RecipeImport()
        {
            checkingTimer.Elapsed += CheckNewRecipes;
            checkingTimer.Enabled = false;
            try {
                using (var stream = new StreamReader(fileName)) {
                    var sDate = stream.ReadLine();
                    DateTime readedTime;
                    if (DateTime.TryParse(sDate, out readedTime)) {
                        SetLastImportedTime(readedTime);
                    }
                }
            }
            catch (Exception ex) {
                Logger.Error("Не удалось прочитать дату последнего импорта данных. Будет установлена минимальная дата.");
                SetLastImportedTime(DateTime.Now.AddYears(-50));
            }

            BuildCommands();
        }

        /// <summary>
        /// Установить состояние. 
        /// </summary>
        /// <param name="aActive">Состояние. True - если запустить, false - остановить.</param>
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
        /// Вернуть время последнего импортированного рецепта.
        /// </summary>
        /// <returns>Время.</returns>
        public DateTime GetLastImportedTime()
        {
            return lastImportedTime;
        }

        /// <summary>
        /// Установить время последнего импортированного рецепта.
        /// </summary>
        /// <param name="aTime">Время.</param>
        public void SetLastImportedTime(DateTime aTime)
        {
            lastImportedTime = aTime;
        }

        /// <summary>
        /// Установить интервал сканирования таблицы на появление новых рецептов.
        /// </summary>
        /// <param name="aInterval">Интервал в миллисекундах.</param>
        public void SetCheckingInterval(double aInterval)
        {
            checkingTimer.Interval = aInterval;
        }

        /// <summary>
        /// Установить ConnectionHolder для Wagstaff.
        /// </summary>
        /// <param name="aConnection">ConnectionHolder.</param>
        public void SetSqlConnection(MsSqlConnectionHolder aConnection)
        {
            sqlConnection = aConnection;
        }

        /// <summary>
        /// Установить ConnectionHolder для ИТЦ.
        /// </summary>
        /// <param name="aConnection">ConnectionHolder.</param>
        public void SetOracleConnection(OracleConnectionHolder aConnection)
        {
            oracleConnection = aConnection;
        }

        /// <summary>
        /// Сохранить на диск время последнего импортированного рецепта.
        /// </summary>
        private void SaveLastImportedTime()
        {
            using (var writer = new StreamWriter(fileName)) {
                writer.WriteLine(GetLastImportedTime().ToString("dd/MM/yyyy HH:mm:ss.FFF"));                                
            }
        }

        /// <summary>
        /// Сканировать таблицу в Wagstaff на наличие новых рецептов. Если появились,
        /// то записать в ИТЦ. Метод вызывается таймером.
        /// </summary>
        /// <param name="sender">Object.</param>
        /// <param name="e">ElapsedEventArgs.</param>
        private void CheckNewRecipes(object sender = null, ElapsedEventArgs e = null)
        {
            var importedRecipes = new List<RecipeInfo>();

            try {                                
                sqlConnection.LockConnection();                                
                Logger.Debug("Проверка новых рецептов");
                // загружаем все записи, которые появились после lastImportedTime
                selectGetNewRicipes.Connection = sqlConnection.GetSqlConnection();

                selectGetNewRicipes.Parameters["DateCreated"].Value = lastImportedTime;
                using (var reader = selectGetNewRicipes.ExecuteReader()) {
                    sqlConnection.UpdateLastOperationTime();
                    while (reader.Read()) {
                        var recipeInfo = new RecipeInfo {
                            Name = reader.GetString(0),
                            Revision = Convert.ToInt32(reader.GetValue(1)),
                            Technology = reader.GetString(2),
                            Alloy = reader.GetString(3),
                            CreationTime = reader.GetDateTime(4),
                            IsArchived = Convert.ToInt32(reader.GetValue(5)) == 1
                        };

                        Logger.Debug("Новый рецепт получен");                            

                        if (TryImportNewRecipe(recipeInfo)) {
                            SetLastImportedTime(recipeInfo.CreationTime);
                            SaveLastImportedTime();
                            importedRecipes.Add(recipeInfo);
                        }
                        else {
                            Logger.Debug("Новый рецепт не сохранен");
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

            TryMarkRecipsAsArchived(importedRecipes);
        }

        /// <summary>
        /// Для всех полученных новых рецептов проверить наличие рецептов с такими же именами
        /// и проверить состояние их бита архивности. Для синхронизации архивных рецептов.
        /// </summary>
        /// <param name="importedRecipes">Список полученных рецептов.</param>
        private void TryMarkRecipsAsArchived(List<RecipeInfo> importedRecipes)
        {
            // получаем все рецепты с одинаковыми именами и устанавливаем для них бит архивности
            try {
                sqlConnection.LockConnection();
                selectGetRecipesUpdateArcived.Connection = sqlConnection.GetSqlConnection();
                Logger.Debug("Проверка на изменение архивности рецептов");                
                foreach (var recipe in importedRecipes) {
                    selectGetRecipesUpdateArcived.Parameters["Name"].Value = recipe.Name;
                    selectGetRecipesUpdateArcived.Parameters["Revision"].Value = recipe.Revision;                                        
                    using (var reader = selectGetRecipesUpdateArcived.ExecuteReader()) {
                        sqlConnection.UpdateLastOperationTime();
                        while (reader.Read()) {
                            var recipeInfo = new RecipeInfo {
                                Name = reader.GetString(0),
                                Revision = Convert.ToInt32(reader.GetValue(1)),
                                IsArchived = Convert.ToInt32(reader.GetValue(2)) == 1
                            };

                            MarkRecipeAsArchived(recipeInfo);
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
        }

        /// <summary>
        /// Пометить указанный рецепт как архивный в таблице ИТЦ.
        /// </summary>
        /// <param name="aRecipeInfo">Рецепт.</param>
        private void MarkRecipeAsArchived(RecipeInfo aRecipeInfo)
        {
            try {
                Logger.Debug("Получен рецепт для изменения бита архивности");

                oracleConnection.LockConnection();
                updateRecipeArchived.Connection = oracleConnection.GetOracleConnection();                
                updateRecipeArchived.Parameters["Archived"].Value = aRecipeInfo.IsArchived ? 1 : 0;
                updateRecipeArchived.Parameters["Name"].Value = aRecipeInfo.Name;
                updateRecipeArchived.Parameters["Revision"].Value = aRecipeInfo.Revision;

                updateRecipeArchived.ExecuteNonQuery();                
            }
            catch (Exception ex) {
                oracleConnection.ProcessError(ex);
            }
            finally {
                oracleConnection.ReleaseConnection();
            }
        }

        /// <summary>
        /// Записать новый рецепт в ИТЦ.
        /// </summary>
        /// <param name="aRecipeInfo">Рецепт.</param>
        /// <returns>True - если запись прошла успешно, false - иначе.</returns>
        private bool TryImportNewRecipe(RecipeInfo aRecipeInfo)
        {
            try {                
                oracleConnection.LockConnection();
                insertImportNewRecipe.Connection = oracleConnection.GetOracleConnection();
                insertImportNewRecipe.Parameters["Name"].Value = aRecipeInfo.Name;
                insertImportNewRecipe.Parameters["Revision"].Value = aRecipeInfo.Revision;
                insertImportNewRecipe.Parameters["Archived"].Value = aRecipeInfo.IsArchived ? 1 : 0;
                Logger.Debug("Импортируется рецепт");
                // сделать новую запись в таблицу RECIPE_INFO
                                
                insertImportNewRecipe.ExecuteNonQuery();
                oracleConnection.UpdateLastOperationTime();                

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
        /// Построить sql команды, используемые в текущем классе.
        /// </summary>
        private void BuildCommands()
        {
            selectGetNewRicipes = new SqlCommand {
                CommandText = "select RCPD_MSC_Name, " +
                              "       RCPD_MSC_REV, " +
                              "       RCPD_MSC_MoldTech, " +
                              "       RCPD_MSC_Alloy, " +
                              "       RCPD_MSC_DateCreated, " +
                              "       RCPD_MSC_Archived " +
                              "from tblRecipe " +
                              "where RCPD_MSC_DateCreated > @DateCreated " +
                              "order by RCPD_MSC_DateCreated"
            };
            selectGetNewRicipes.Parameters.Add("DateCreated", SqlDbType.DateTime);

            selectGetRecipesUpdateArcived = new SqlCommand {
                CommandText = "select RCPD_MSC_Name, " +
                              "       RCPD_MSC_REV, " +
                              "       RCPD_MSC_Archived " +
                              "from tblRecipe " +
                              "where RCPD_MSC_Name=@Name and RCPD_MSC_REV<>@Revision"                              
            };
            selectGetRecipesUpdateArcived.Parameters.Add("Name", SqlDbType.VarChar);
            selectGetRecipesUpdateArcived.Parameters.Add("Revision", SqlDbType.Int);

            updateRecipeArchived = new OracleCommand {
                CommandText = "update RECIPE_INFO " +
                              "set ARCHIVED=:Archived " +
                              "where NAME=:Name and " +
                              "      REVISION=:Revision"
            };
            updateRecipeArchived.Parameters.Add("Archived", OracleDbType.Int32);
            updateRecipeArchived.Parameters.Add("Name", OracleDbType.Varchar2);
            updateRecipeArchived.Parameters.Add("Revision", OracleDbType.Int32);

            insertImportNewRecipe = new OracleCommand {
                CommandText = "insert into RECIPE_INFO(name, revision, archived) " +
                              "values(:Name, :Revision, :Archived)"
            };
            insertImportNewRecipe.Parameters.Add("Name", OracleDbType.Varchar2);
            insertImportNewRecipe.Parameters.Add("Revision", OracleDbType.Int32);
            insertImportNewRecipe.Parameters.Add("Archived", OracleDbType.Int32);
        }
    }
}
