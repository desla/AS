using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using log4net;
using log4net.Config;

namespace Alvasoft.AudioServer
{
    /// <summary>
    /// Осуществляет запуск программы.
    /// </summary>
    class Program : ServiceBase
    {
        private static readonly ILog Logger = LogManager.GetLogger("Program");

        private static AudioServer audioServerInstance = new AudioServer();
        private static Thread serviceThread;

        /// <summary>
        /// Исполняемая функция.
        /// </summary>
        /// <param name="args">Параметрвы.</param>
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0 || args[0] != "console") {
                ServiceBase.Run(new Program());
            }
            else {
                serviceThread = new Thread(ServiceMethod);
                serviceThread.Start();

                Console.ReadLine();

                audioServerInstance.Uninitialize();
                Thread.Sleep(1000);
                if (serviceThread.IsAlive) {
                    serviceThread.Abort();
                }
            }
        }

        /// <summary>
        /// Запускает сервис.
        /// </summary>
        /// <param name="args">Параметры.</param>
        protected override void OnStart(string[] args)
        {
            serviceThread = new Thread(ServiceMethod);
            serviceThread.Start();
        }        

        /// <summary>
        /// Останавливает сервис.
        /// </summary>
        protected override void OnStop()
        {
            audioServerInstance.Uninitialize();
            Thread.Sleep(1000);
            if (serviceThread.IsAlive) {
                serviceThread.Abort();
            }            
        }

        /// <summary>
        /// Запускает в работу аудиосервер.
        /// </summary>
        private static void ServiceMethod()
        {
            var appPath = Application.StartupPath + "\\";

            audioServerInstance = new AudioServer();

            var configLogingFileName = appPath + "Configurations\\Logging.xml";            
            XmlConfigurator.Configure(new FileInfo(configLogingFileName));

            Logger.Info("Аудио-сервер.");

            audioServerInstance.Initialize();

            Logger.Info("Поехали...");

            while (audioServerInstance.IsInitialized()) {
                Thread.Sleep(1000);
            }
        }
    }
}
