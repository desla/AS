using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using log4net;
using log4net.Config;
using NAudio.Wave;

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
        private static Thread garbageCollectorThread;

        /// <summary>
        /// Исполняемая функция.
        /// </summary>
        /// <param name="args">Параметрвы.</param>      
        [STAThread]                  
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0) {
                if (args[0].ToLower() == "console") {
                    if (args.Length > 1 && args[1].ToLower() == "configure") {
                        AsioConfigure();
                        return;
                    }

                    GarbageCollectorStart();

                    serviceThread = new Thread(ServiceMethod);
                    serviceThread.SetApartmentState(ApartmentState.STA);
                    serviceThread.Start();

                    Console.ReadLine();

                    audioServerInstance.Uninitialize();
                    Thread.Sleep(1000);
                    if (serviceThread.IsAlive) {
                        serviceThread.Abort();
                    }

                    if (garbageCollectorThread.IsAlive) {
                        garbageCollectorThread.Abort();
                    }
                }                
            }
            else {
                ServiceBase.Run(new Program());
            }
        }

        private static void GarbageCollectorStart()
        {
            garbageCollectorThread = new Thread(() => {
                while (true) {
                    Thread.Sleep(60000);                                        
                    GC.Collect();
                }
            });
            garbageCollectorThread.Start();
        }

        private static void AsioConfigure()
        {            
            Console.WriteLine("Вы запустили AudioServer в режиме конфигурации Asio-драйверов.");
            if (!AsioOut.isSupported()) {
                Console.WriteLine("В системе нет установленных asio-драйверов.");
                return;
            }
            Console.WriteLine("Выберите драйвер из списка и нажмите Enter для вызова панели настроек.");
            Console.WriteLine("Для выхода введите -1.");
            Console.WriteLine("Установленные asio-драйвера:");
            var drivers = AsioOut.GetDriverNames();
            for (var i = 0; i < drivers.Length; ++i) {
                Console.WriteLine("{0}. {1}", i + 1, drivers[i]);
            }
            var driverIndex = 0;
            AsioOut asioOut = null;
            while (true) {
                if (asioOut != null) {
                    asioOut.Dispose();
                }

                Console.Write("Введите номер драйвера: ");
                driverIndex = Convert.ToInt32(Console.ReadLine());
                if (driverIndex == -1) {
                    break;
                }

                if (driverIndex < 1 || driverIndex > drivers.Length) {
                    Console.WriteLine("Вы ввели некорректный номер драйвера.");
                    continue;                    
                }                               

                var driverName = drivers[driverIndex - 1];
                asioOut = new AsioOut(driverName);
                Console.WriteLine("Запускаем панель настроек для драйвера " + driverName);                
                asioOut.ShowControlPanel();                
            } 
        }

        /// <summary>
        /// Запускает сервис.
        /// </summary>
        /// <param name="args">Параметры.</param>
        protected override void OnStart(string[] args)
        {
            GarbageCollectorStart();

            serviceThread = new Thread(ServiceMethod);
            serviceThread.SetApartmentState(ApartmentState.STA);
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

            if (garbageCollectorThread.IsAlive) {
                garbageCollectorThread.Abort();
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
