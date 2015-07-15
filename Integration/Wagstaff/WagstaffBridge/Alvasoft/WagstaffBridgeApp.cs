using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using log4net.Config;

namespace Alvasoft.WagstaffBridge
{    
    /// <summary>
    /// Класс приложения.
    /// </summary>
    class WagstaffBridgeApp : ServiceBase
    {
        private static Thread serviceThread;     
        private static WagstaffBridge bridge = new WagstaffBridge();

        /// <summary>
        /// Главная функция.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args != null && args.Length != 0) {
                if (args[0].ToLower().Equals("console")) {
                    serviceThread = new Thread(WagstaffBridgeMethod);
                    Console.WriteLine("Запускаем WagstaffBridge... Для выхода нажмите Enter.");
                    serviceThread.Start();
                    Console.ReadLine();

                    bridge.Uninitialize();
                    Thread.Sleep(1000);
                    if (serviceThread.IsAlive) {
                        serviceThread.Abort();
                    }
                }
                else {
                    Console.WriteLine("Для запуска консольного приложения используйте параметра 'console'.");
                }
            }
            else {
                ServiceBase.Run(new WagstaffBridgeApp());
            }
        }

        protected override void OnStart(string[] args)
        {
            serviceThread = new Thread(WagstaffBridgeMethod);
            serviceThread.Start();
        }        

        protected override void OnStop()
        {
            bridge.Uninitialize();
            Thread.Sleep(1000);
            if (serviceThread.IsAlive) {
                serviceThread.Abort();
            }
        }

        private static void WagstaffBridgeMethod()
        {
            var appPath = Application.StartupPath + "\\";
            // Конфигурация логирования.            
            var configLogingFileName = appPath + "Configuration\\Logging.xml";            

            XmlConfigurator.Configure(new FileInfo(configLogingFileName));

            bridge.Initialize();

            while (bridge.IsInitialized()) {
                Thread.Sleep(1000);
            }
        }
    }
}
