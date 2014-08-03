using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using IctBaden.Framework.AppUtils;

namespace Monolith
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("kernel32", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId);
// ReSharper disable once InconsistentNaming
        const uint ATTACH_PARENT_PROCESS = 0x0ffffffff;  // default value if not specifing a process ID

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                // Command line given, display console
                if (!AttachConsole(ATTACH_PARENT_PROCESS))
                { // Attach to an parent process console
                    if (!AllocConsole()) // Alloc a new console
                    {
                        MessageBox.Show(":-(");
                    }
                }

                Current.ShutdownMode = ShutdownMode.OnExplicitShutdown; 
                var color = Console.ForegroundColor;
                ConsoleMain(e.Args);
                Console.ForegroundColor = color;
                return;
            }

            base.OnStartup(e);
        }

        private static void ConsoleMain(string[] args)
        {
            Console.WriteLine(@"Monolith V" + AssemblyInfo.Default.DisplayVersion);

            var exit = false;
            var options = new OptionSet
            {
                { "h|?|help", v =>
                {
                    exit = true;
                    ShowHelp();
                } },
            };

            var fileNames = options.Parse(args);

            if (exit)
            {
                Current.Shutdown(0);
                return;
            }

            string exeName = null;
            if (fileNames.Count > 0)
            {
                exeName = fileNames[0];
            }
            if (string.IsNullOrEmpty(exeName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(@"An EXE file has to be given.");
                Current.Shutdown(2);
                return;
            }
            if (!File.Exists(exeName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(@"EXE file not found: " + fileNames[0]);
                Current.Shutdown(2);
                return;
            }

            Console.WriteLine(@"Packing EXE file: " + exeName);

            Current.Shutdown(0);
        }

        private static void ShowHelp()
        {
            Console.WriteLine(@"");
            Console.WriteLine(@"Monolith <ExeFileName>");
            Console.WriteLine(@"");
            Console.WriteLine(@"Options");
            Console.WriteLine(@"/h[elp] or /?   Show help");
        }

    }
}
