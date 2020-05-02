using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public LightModel[] Outputs { get; } = Enumerable.Range(0, 17).Select(idx => new LightModel { Text = $"O{idx + 1}" }).ToArray();

        public Logic Logic { get; private set; }

        public static Task<TaskScheduler> GetTaskSchedulerAsync(
            Dispatcher dispatcher,
            DispatcherPriority priority = DispatcherPriority.Normal)
        {

            var taskCompletionSource = new TaskCompletionSource<TaskScheduler>();
            var invocation = dispatcher.BeginInvoke(new Action(() =>
              taskCompletionSource.SetResult(
                  TaskScheduler.FromCurrentSynchronizationContext())), priority);

            invocation.Aborted += (s, e) =>
                taskCompletionSource.SetCanceled();

            return taskCompletionSource.Task;
        }

        private DispatcherTimer Timer;

        public MainWindow()
        {
            InitializeComponent();

            GetTaskSchedulerAsync(Dispatcher)
                .ContinueWith(t =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        Logic = new Logic(Inputs, Outputs, t.Result);

                        var stopwatch = Stopwatch.StartNew();
                        Timer = new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal,
                            (s, e) => Logic.Process(stopwatch.Elapsed), Dispatcher);
                        Timer.Start();

                        DataContext = this;
                    });
                });
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
