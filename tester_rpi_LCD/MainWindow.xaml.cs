using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using tester_rpi_LCD.Model;

namespace tester_rpi_LCD
{
    public class MainWindow : Window
    {
        readonly DispatcherTimer LiveValuesTimer;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            var mainModel = new MainModel { Logic = Program.Logic };
            DataContext = mainModel;
            Opened += (s, e) => WindowState = WindowState.Maximized;

            LiveValuesTimer = new DispatcherTimer(TimeSpan.FromTicks((long)(1.0 / 10.0 * TimeSpan.TicksPerSecond)), DispatcherPriority.SystemIdle,
                (s, e) => ((MainModel)DataContext).UpdateLiveValues());
            LiveValuesTimer.Start();

            new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            var ips = new HashSet<IPAddress>();
                            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                                if (ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                                    foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                                        if (ip.Address?.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                            ips.Add(ip.Address);

                            Dispatcher.UIThread.InvokeAsync(() =>
                                mainModel.IpAddresses = ips.OrderBy(w => Version.Parse(w.ToString())).ToArray());
                        }
                        catch { }

                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    }
                })
            { IsBackground = true, Name = "IP Address Reader thread" }.Start();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
