using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using Alvasoft.BridgeConfiguration;
using Alvasoft.Mossner;
using log4net.Config;

namespace Alvasoft
{
    /// <summary>
    /// Класс приложения.
    /// </summary>
    class MossnerBridgeApp : ServiceBase
    {
        private static Thread serviceThread;
        private static MossnerBridge bridge;

        /// <summary>
        /// Главная функция.
        /// </summary>
        /// <param name="args">Аргументы.</param>
        static void Main(string[] args)
        {
            if (args != null && args.Length != 0) {
                if (args[0].ToLower().Equals("console")) {
                    serviceThread = new Thread(MossnerBridgeMethod);

                    Console.WriteLine("Запускаем MossnerBridge... Для выхода нажмите Enter.");

                    serviceThread.Start();

                    Console.ReadLine();

                    bridge.Uninitialize();
                    Thread.Sleep(1000);
                    if (serviceThread.IsAlive) {
                        serviceThread.Abort();
                    }
                }
                else {
                    Console.WriteLine("Для запуска консольного приложения используйте параметр 'console'.");
                }
            }
            else {
                ServiceBase.Run(new MossnerBridgeApp());
            }
        }

        protected override void OnStart(string[] args)
        {
            serviceThread = new Thread(MossnerBridgeMethod);
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

        private static void MossnerBridgeMethod()
        {
            var appPath = Application.StartupPath + "\\";
            var configLogingFileName = appPath + "Configuration\\Logging.xml";            
            XmlConfigurator.Configure(new FileInfo(configLogingFileName));            

            var configuration = new MossnerBridgeConfiguration();
            configuration.LoadConnectionsConfiguration(appPath + "Configuration\\Network.xml");

            bridge = new MossnerBridge(configuration);
            bridge.Initialize();

            while (bridge.IsInitialized()) {
                Thread.Sleep(1000);
            }
        }             
    }
}
