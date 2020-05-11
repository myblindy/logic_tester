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
            MainModel mainModel = new MainModel();
            DataContext = mainModel;
            mainModel.Counter = 2356788;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
