using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using OPCAutomation;
using Oracle.ManagedDataAccess.Client;
using StableConnection.ConnectionHolder;
using StableConnection.ConnectionHolder.ConnectionCallback.Impl;

namespace StableConnection
{
    class Program
    {
        static void Main(string[] args)
        {
            //OracleTest();
            OpcTest();
            //SqlTest();
        }

        private static void OracleTest()
        {
            var connection = new OracleConnectionHolder("192.168.0.108", "customer", "customer");
            connection.SetCallback(new UniversalConnectionHolderCallback());

            connection.OpenConnection();

            var commandCount = 0;

            while (true) {
                try {
                    if (connection.IsConnected()) {
                        using (var command = new OracleCommand("select * from DEMO_USERS", connection.GetOracleConnection()))
                        using (var reader = command.ExecuteReader()) {
                            var count = 0;
                            while (reader.Read()) {
                                count++;
                            }
                            Console.WriteLine(commandCount++ + ") Команда выполнена. Count = " + count);
                        }
                    }
                }
                catch (Exception ex) {
                    if (!connection.ProcessError(ex)) {
                        throw;
                    }
                }
                finally {
                    Thread.Sleep(1000);
                }
            }
        }

        private static void OpcTest()
        {
            var host = "192.168.0.108";
            //var host = "10.99.4.225";
            var connection = new OpcConnectionHolder("RSLinx OPC Server", host);
            connection.SetCallback(new UniversalConnectionHolderCallback());

            connection.OpenConnection();

            OPCGroup group = null;
            OPCItem item = null;

            while (true) {
                try {
                    if (connection.IsConnected()) {
                        if (group == null) {
                            group = connection.GetOpcServer().OPCGroups.Add("hello");
                            item = group.OPCItems.AddItem("[MOSSNER2]L3_Interface.OUT.LiveBit", 0);
                        }

                        object value, quality, timeStamp;
                        item.Read(0, out value, out quality, out timeStamp);
                        connection.UpdateLastOperationTime();
                        Console.WriteLine("Item: {0} TimeStamp: {1} Value: {2} Quality: {3}",
                            item.ItemID, timeStamp, value, quality);
                    }
                }
                catch (Exception ex) {
                    group = null;
                    item = null;
                    if (!connection.ProcessError(ex)) {
                        throw;
                    }
                }
                finally {
                    Thread.Sleep(1000);
                }                
            }
        }

        private static void SqlTest()
        {
            //var connection = new MsSqlConnectionHolder("WAGTS0", "Wagstaff", "customer", "customer"); // table : tblCastNumbers
            var connection = new MsSqlConnectionHolder("192.168.0.105\\SQLEXPRESS", "VirtualDatabase", "Den", "Den");
            connection.SetCallback(new UniversalConnectionHolderCallback());

            connection.OpenConnection();

            var commandCount = 0;

            while (true) {
                try {                    
                    if (connection.IsConnected()) {                        
                        using (var command = new SqlCommand("select * from VirtualTable", connection.GetSqlConnection()))
                        using (var reader = command.ExecuteReader()) {
                            var count = 0;
                            while (reader.Read()) {
                                count++;                            
                            }
                            Console.WriteLine(commandCount++ + ") Команда выполнена. Count = " + count);
                        }                                                
                    }
                }
                catch(Exception ex) {
                    if (!connection.ProcessError(ex)) {
                        throw;
                    }
                }
                finally {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
