using System;
using System.IO;
using System.Runtime.InteropServices;

using log4net;
using log4net.Config;

using ACE.Common;
using ACE.Database;
using ACE.DatLoader;
using ACE.Server.Command;
using ACE.Server.Managers;
using ACE.Server.Network.Managers;
using ACE.PcapReader;

namespace ACE.Server
{
    class Program
    {
        /// <summary>
        /// The timeBeginPeriod function sets the minimum timer resolution for an application or device driver. Used to manipulate the timer frequency.
        /// https://docs.microsoft.com/en-us/windows/desktop/api/timeapi/nf-timeapi-timebeginperiod
        /// Important note: This function affects a global Windows setting. Windows uses the lowest value (that is, highest resolution) requested by any process.
        /// </summary>
        [DllImport("winmm.dll", EntryPoint="timeBeginPeriod")]
        public static extern uint MM_BeginPeriod(uint uMilliseconds);

        /// <summary>
        /// The timeEndPeriod function clears a previously set minimum timer resolution
        /// https://docs.microsoft.com/en-us/windows/desktop/api/timeapi/nf-timeapi-timeendperiod
        /// </summary>
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint MM_EndPeriod(uint uMilliseconds);

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            // Init our text encoding options. This will allow us to use more than standard ANSI text, which the client also supports.
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            // Do system specific initializations here
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // On many windows systems, the default resolution for Thread.Sleep is 15.6ms. This allows us to command a tighter resolution
                    MM_BeginPeriod(1);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }

            Console.WriteLine("Starting PCAP Player...");
            Console.Title = @"PCAP Player";

            Console.WriteLine("Initializing ConfigManager...");
            ConfigManager.Initialize();

            Console.WriteLine("Initializing ServerManager...");
            ServerManager.Initialize();

            //log.Info("Initializing DatManager...");
            //DatManager.Initialize(ConfigManager.Config.Server.DatFilesDirectory, true);

            //log.Info("Initializing DatabaseManager...");
            //DatabaseManager.Initialize();

            //log.Info("Starting DatabaseManager...");
            //DatabaseManager.Start();

            Console.WriteLine("Starting PropertyManager...");
            PropertyManager.Initialize();

            //log.Info("Initializing GuidManager...");
            //GuidManager.Initialize();

            //log.Info("Precaching World Database Disabled...");

            Console.WriteLine("Initializing PlayerManager...");
            PlayerManager.Initialize();

            //log.Info("Initializing HouseManager...");
            //HouseManager.Initialize();

            Console.WriteLine("Initializing InboundMessageManager...");
            InboundMessageManager.Initialize();

            Console.WriteLine("Initializing SocketManager...");
            SocketManager.Initialize();

            Console.WriteLine("Initializing WorldManager...");
            WorldManager.Initialize();

            Console.WriteLine("Initializing PCapReader...");
            PCapReader.Initialize();

            log.Info("Initializing EventManager...");
            EventManager.Initialize();

            WorldManager.Open(null);

            // This should be last
            Console.WriteLine("Initializing CommandManager...");
            CommandManager.Initialize();

        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error(e.ExceptionObject);
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            /*
            if (!ServerManager.ShutdownInitiated)
                log.Warn("Unsafe server shutdown detected! Data loss is possible!");

            PropertyManager.StopUpdating();
            DatabaseManager.Stop();
            */
            // Do system specific cleanup here
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    MM_EndPeriod(1);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }
    }
}
