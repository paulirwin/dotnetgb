using System;
using System.Windows;

namespace DotNetGB.WpfGui
{
    public partial class App : Application
    {
        public App()
        {
            Startup += OnStartup;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 1 && e.Args[0].ToLower() == "--help")
            {
                PrintUsage();
                Environment.Exit(0);
                return;
            }

            new MainWindow(e.Args).Show();
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("DotNetGBWin.exe [OPTIONS] ROM_FILE");
            Console.WriteLine();
            Console.WriteLine("Available options:");
            Console.WriteLine("  -d  --force-dmg                Emulate classic GB (DMG) for universal ROMs");
            Console.WriteLine("  -c  --force-cgb                Emulate color GB (CGB) for all ROMs");
            Console.WriteLine("  -b  --use-bootstrap            Start with the GB bootstrap");
            Console.WriteLine("  -db --disable-battery-saves    Disable battery saves");
            Console.WriteLine("      --debug                    Enable debug console");
            Console.WriteLine("      --headless                 Start in the headless mode");
        }
    }
}
