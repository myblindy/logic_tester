using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Threading.Tasks;
using tester_rpi_LCD.Model;

namespace tester_rpi_LCD
{
    public class MainWindow : Window
    {
        readonly DispatcherTimer Timer;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            DataContext = new MainModel { Logic = Program.Logic };
            Opened += (s, e) => WindowState = WindowState.Maximized;

            Timer = new DispatcherTimer(TimeSpan.FromTicks((long)(1.0 / 10.0 * TimeSpan.TicksPerSecond)), DispatcherPriority.SystemIdle,
                (s, e) => ((MainModel)DataContext).UpdateLiveValues());
            Timer.Start();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
