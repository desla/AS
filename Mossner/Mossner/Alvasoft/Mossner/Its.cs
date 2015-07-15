using System;
using System.Data;
using Alvasoft.ConnectionHolder;
using log4net;
//using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;


namespace Alvasoft.Mossner
{   
    /// <summary>
    /// Заводская ИТС.
    /// </summary>
    public class Its
    {
        private static readonly ILog Logger = LogManager.GetLogger("Its");

        private OracleConnectionHolder oracleConnection;
        private OracleCommand checkMelt;
        private OracleCommand checkSlab;
        private OracleCommand addSlab;
        private OracleCommand checkProd;
        private OracleCommand addProd;

        /// <summary>
        /// Конструктор.
        /// </summary>
        public Its()
        {
            BuildCommands();
        }        

        /// <summary>
        /// Устанавливает соединение с базой данных.
        /// </summary>
        /// <param name="aOracleConnection">Соединение с БД.</param>
        public void SetOracleHolder(OracleConnectionHolder aOracleConnection)
        {
            oracleConnection = aOracleConnection;
        }

        /// <summary>
        /// Проверяет плавку.
        /// </summary>
        /// <param name="aMixerNumber">Номер миксера.</param>
        /// <param name="aMeltNumber">Номер плавки.</param>
        /// <returns>1 - OK, 2 - нет данной плавки.</returns>
        public int CheckMelt(int aMixerNumber, int aMeltNumber)
        {
            var result = 0;
            try {
                oracleConnection.LockConnection();
                Logger.Info("Проверка плавки.");

                checkMelt.Connection = oracleConnection.GetOracleConnection();
                checkMelt.Parameters["mixerNumber"].Value = aMixerNumber;
                checkMelt.Parameters["meltNumber"].Value = aMeltNumber;

                using (var reader = checkMelt.ExecuteReader()) {
                    oracleConnection.UpdateLastOperationTime();
                    while (reader.Read()) {
                        result = Convert.ToInt32(reader.GetValue(0));
                    }
                }
            }
            catch (Exception ex) {
                oracleConnection.ProcessError(ex);
            }
            finally {
                oracleConnection.ReleaseConnection();
            }

            return result;
        }

        /// <summary>
        /// Проверяет слиток.
        /// </summary>
        /// <param name="aMixerNumber">Номер миксера.</param>
        /// <param name="aMeltNumber">Номер плавки.</param>
        /// <param name="aSlabNumber">Номер слитка.</param>
        /// <returns>1 - OK, 3 - такой слиток уже есть.</returns>
        public int CheckSlab(int aMixerNumber, int aMeltNumber, int aSlabNumber)
        {
            var result = 0;
            try {
                oracleConnection.LockConnection();
                Logger.Info("Проверка слитка.");

                checkSlab.Connection = oracleConnection.GetOracleConnection();
                checkSlab.Parameters["mixerNumber"].Value = aMixerNumber;
                checkSlab.Parameters["meltNumber"].Value = aMeltNumber;
                checkSlab.Parameters["slabNumber"].Value = aSlabNumber;

                using (var reader = checkSlab.ExecuteReader()) {
                    oracleConnection.UpdateLastOperationTime();
                    while (reader.Read()) {
                        result = Convert.ToInt32(reader.GetValue(0));
                    }
                }
            }
            catch (Exception ex) {
                oracleConnection.ProcessError(ex);
            }
            finally {
                oracleConnection.ReleaseConnection();
            }

            return result;
        }

        /// <summary>
        /// Добавлет слиток.
        /// </summary>
        /// <param name="aMixerNumber">Номер миксера.</param>
        /// <param name="aMeltNumber">Номер плавки.</param>
        /// <param name="aSlabNumber">Номер слитка.</param>
        public void AddSlab(int aMixerNumber, int aMeltNumber, int aSlabNumber)
        {
            try {
                oracleConnection.LockConnection();
                Logger.Info("Добавление слитка.");

                addSlab.Connection = oracleConnection.GetOracleConnection();
                addSlab.Parameters["mixerNumber"].Value = aMixerNumber;
                addSlab.Parameters["meltNumber"].Value = aMeltNumber;
                addSlab.Parameters["slabNumber"].Value = aSlabNumber;

                addSlab.ExecuteNonQuery();
                oracleConnection.UpdateLastOperationTime();
            }
            catch (Exception ex) {
                oracleConnection.ProcessError(ex);
            }
            finally {
                oracleConnection.ReleaseConnection();
            }
        }

        /// <summary>
        /// Проверяет продукт
        /// </summary>
        /// <param name="aMixerNumber">Номер микксера.</param>
        /// <param name="aMeltNumber">Номер плавки.</param>
        /// <param name="aSlabNumber">Номер слитка.</param>
        /// <returns></returns>
        public int CheckProd(int aMixerNumber, int aMeltNumber, int aSlabNumber)
        {
            var result = 0;
            try {
                oracleConnection.LockConnection();
                Logger.Info("Проверка Prod.");

                checkProd.Connection = oracleConnection.GetOracleConnection();
                checkProd.Parameters["mixerNumber"].Value = aMixerNumber;
                checkProd.Parameters["meltNumber"].Value = aMeltNumber;
                checkProd.Parameters["slabNumber"].Value = aSlabNumber;

                using (var reader = checkProd.ExecuteReader()) {
                    oracleConnection.UpdateLastOperationTime();
                    while (reader.Read()) {
                        result = Convert.ToInt32(reader.GetValue(0));
                    }
                }
            }
            catch (Exception ex) {
                oracleConnection.ProcessError(ex);
            }
            finally {
                oracleConnection.ReleaseConnection();
            }

            return result;
        }

        /// <summary>
        /// Добавляет продукт.
        /// </summary>
        /// <param name="aMixerNumber">Номер миксера.</param>
        /// <param name="aMeltNumber">Номер плавки.</param>
        /// <param name="aSlabNumber">Номер слитка.</param>
        /// <param name="aSlabWeight">Вес слитка.</param>
        /// <param name="aSlabLength">Длина слитка.</param>
        public void AddProd(int aMixerNumber, int aMeltNumber, int aSlabNumber, int aSlabWeight, int aSlabLength)
        {
            try {
                oracleConnection.LockConnection();
                Logger.Info("Добавление Prod.");

                addProd.Connection = oracleConnection.GetOracleConnection();
                addProd.Parameters["mixerNumber"].Value = aMixerNumber;
                addProd.Parameters["meltNumber"].Value = aMeltNumber;
                addProd.Parameters["slabNumber"].Value = aSlabNumber;
                addProd.Parameters["slabWeight"].Value = aSlabWeight;
                addProd.Parameters["slabLength"].Value = aSlabLength;

                addProd.ExecuteNonQuery();
                oracleConnection.UpdateLastOperationTime();
            }
            catch (Exception ex) {
                oracleConnection.ProcessError(ex);
            }
            finally {
                oracleConnection.ReleaseConnection();
            }
        }

        /// <summary>
        /// Строит Oracle-команды.
        /// </summary>
        private void BuildCommands()
        {
            checkMelt = new OracleCommand {
                CommandText = "select Chis.On_Weight.checkMelt(:mixerNumber, :meltNumber) as Result from DUAL"
                //CommandText = "select customer.checkMelt(:mixerNumber, :meltNumber) as Result from DUAL"
            };
            checkMelt.Parameters.Add("mixerNumber", OracleDbType.Int32);
            checkMelt.Parameters.Add("meltNumber", OracleDbType.Int32);

            checkSlab = new OracleCommand {
                CommandText = "select Chis.On_Weight.checkSlab(:mixerNumber, :meltNumber, :slabNumber) as Result from DUAL"
                //CommandText = "select customer.checkSlab(:mixerNumber, :meltNumber, :slabNumber) as Result from DUAL"
            };
            checkSlab.Parameters.Add("mixerNumber", OracleDbType.Int32);
            checkSlab.Parameters.Add("meltNumber", OracleDbType.Int32);
            checkSlab.Parameters.Add("slabNumber", OracleDbType.Int32);

            addSlab = new OracleCommand {
                CommandText = "Chis.On_Weight.addSlab"
                //CommandText = "customer.addSlab"
            };
            addSlab.CommandType = CommandType.StoredProcedure;
            addSlab.Parameters.Add("mixerNumber", OracleDbType.Int32);
            addSlab.Parameters.Add("meltNumber", OracleDbType.Int32);
            addSlab.Parameters.Add("slabNumber", OracleDbType.Int32);

            checkProd = new OracleCommand {
                CommandText = "select Chis.On_Weight.checkProd(:mixerNumber, :meltNumber, :slabNumber) as Result from DUAL"
                //CommandText = "select customer.checkProd(:mixerNumber, :meltNumber, :slabNumber) as Result from DUAL"
            };
            checkProd.Parameters.Add("mixerNumber", OracleDbType.Int32);
            checkProd.Parameters.Add("meltNumber", OracleDbType.Int32);
            checkProd.Parameters.Add("slabNumber", OracleDbType.Int32);

            addProd = new OracleCommand {
                CommandText = "Chis.On_Weight.add"
                //CommandText = "customer.add"
            };
            addProd.CommandType = CommandType.StoredProcedure;
            addProd.Parameters.Add("mixerNumber", OracleDbType.Int32);
            addProd.Parameters.Add("meltNumber", OracleDbType.Int32);
            addProd.Parameters.Add("slabNumber", OracleDbType.Int32);
            addProd.Parameters.Add("slabWeight", OracleDbType.Int32);
            addProd.Parameters.Add("slabLength", OracleDbType.Int32);
        }
    }
}
