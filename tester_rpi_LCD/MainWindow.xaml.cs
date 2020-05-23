using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using tester_rpi_LCD.Model;

namespace tester_rpi_LCD
{
    public class MainWindow : Window
    {
        DispatcherTimer Timer;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            DataContext = new MainModel { Logic = Program.Logic };
            WindowOpenedEvent.AddClassHandler(typeof(MainWindow), (s, e) => WindowState = WindowState.Maximized);
            Timer = new DispatcherTimer(TimeSpan.FromMilliseconds(1.0 / 60.0), DispatcherPriority.SystemIdle, (s, e) => ((MainModel)DataContext).UpdateLiveValues());
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
