using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using tester.Support;

namespace tester
{
    public partial class MainWindow : Window
    {
        public LightModel[] Inputs { get; } = Enumerable.Range(0, 5).Select(idx => new LightModel { Text = $"I{idx + 1}" }).ToArray();
        public LightModel[] Outputs { get; } = Enumerable.Range(0, 20).Select(idx => new LightModel { Text = $"O{idx + 1}" }).ToArray();

        public Logic Logic { get; }

        private readonly DispatcherTimer Timer;

        public MainWindow()
        {
            InitializeComponent();
            Logic = new Logic(Inputs, Outputs);

            Timer = new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal, (s,e) => Logic.Process(), Dispatcher);
            Timer.Start();

            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e) => Logic.Reset();
    }

    public class LightModel : ReactiveObject, ILight
    {
        bool active;
        public bool Active { get => active; set => this.RaiseAndSetIfChanged(ref active, value); }

        string text;
        public string Text { get => text; set => this.RaiseAndSetIfChanged(ref text, value); }
    }
}
