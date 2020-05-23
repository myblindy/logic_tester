using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using tester_rpi_LCD.Model;

namespace tester_rpi_LCD
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            DataContext = new MainModel { Logic = Program.Logic };
            WindowOpenedEvent.AddClassHandler(typeof(MainWindow), (s, e) => WindowState = WindowState.Maximized);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
