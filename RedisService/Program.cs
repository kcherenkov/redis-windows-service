using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace RedisService
{
    class Program : ServiceBase
    {
        const string RedisServer = "redis-server.exe";
        const string RedisCLI = "redis-cli.exe";
        static string _path;

        static int _port;

        static void Main(string[] args)
        {
            _path = AppDomain.CurrentDomain.BaseDirectory;
            if (!File.Exists(Path.Combine(_path, RedisServer)))
                Exit("Couldn`t find " + RedisServer);

            if (!File.Exists(Path.Combine(_path, RedisCLI)))
                Exit("Couldn`t find " + RedisCLI);

            if (Environment.UserInteractive)
            {
                SetConsoleCtrlHandler(ConsoleCtrlCheck, true);
                //Console.CancelKeyPress += (sender, eventArgs) => StopRedis();
                StartRedis(args.Length == 1 ? args[0] : null);
            }
            else
                Run(new Program());
        }

        protected override void OnStart(string[] args)
        {
            var arguments = Environment.GetCommandLineArgs();
            if (arguments.Length > 2)
                Exit("Too many arguments");
            base.OnStart(args);
            StartRedis(arguments.Length == 2 ? arguments[1] : null);
        }

        protected override void OnStop()
        {
            base.OnStop();
            StopRedis();
        }

        static void StartRedis(string configPath = null)
        {
            var pi = new ProcessStartInfo(Path.Combine(_path, RedisServer));

            if (configPath != null)
            {
                FindPort(configPath);
                
                // Workaround for spaces in configuration filename.
                pi.Arguments = Path.GetFileName(configPath);
                pi.WorkingDirectory = Path.GetDirectoryName(configPath);
            }

            using (var process = new Process { StartInfo = pi })
            {
                if (process.Start())
                    if (Environment.UserInteractive)
                        process.WaitForExit();
                    else
                    {
                    }
                else
                    Exit("Failed to start Redis process");
            }
        }

        private static void FindPort(string path)
        {
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.IndexOf("port") == 0)
                    {
                        _port = int.Parse(line.Substring(5, line.Length - 5));
                        break;
                    }
                }
                if (_port == 0)
                    Exit("Couldn`t find Redis port in config file");
            }
        }

        static void StopRedis()
        {
            var pi = new ProcessStartInfo(Path.Combine(_path, RedisCLI)) { Arguments = (_port == 0 ? "" : String.Format("-p {0} ", _port)) + "shutdown" };

            if (!(new Process { StartInfo = pi }).Start())
                Exit("Failed to stop Redis process");
        }

        static void Exit(string message)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(message);
                Environment.Exit(-1);
            }
            else
            {
                //File.WriteAllText(Path.Combine(_path, "error.txt"), message);
                throw new ApplicationException(message);
            }
        }

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

        // A delegate type to be used as the handler routine 
        // for SetConsoleCtrlHandler.
        private delegate bool HandlerRoutine(CtrlTypes ctrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        private enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            StopRedis();
            return true;
        }
    }
}
