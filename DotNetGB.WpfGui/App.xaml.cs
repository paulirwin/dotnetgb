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
            new Emulator(e.Args).Run();
        }
    }
}
