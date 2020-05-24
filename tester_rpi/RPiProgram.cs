using System;
using System.Linq;
using System.Device.Gpio;
using System.Diagnostics;
using MoreLinq;
using System.Threading;
using System.Threading.Tasks;

namespace tester_rpi
{
    public class RPiProgram
    {
        //private const int ResetPin = 27;

        //private const int InputPin0 = 22;
        private static int InputPinCount;

        //private const int OutputPin0 = 5;
        private static int OutputPinCount;

        static readonly Stopwatch Stopwatch = new Stopwatch();
        static int Cycles;
        static readonly TimeSpan CycleInterval = TimeSpan.FromSeconds(5);
        static TimeSpan LastCycleEnd = TimeSpan.Zero;

        public static Logic<Light> Logic;
        public static AutoResetEvent LogicInitialized;

        public static void RpiMain(string[] _)
        {

            //var context = new SingleThreadedSynchronizationContext(Thread.CurrentThread);
            //SynchronizationContext.SetSynchronizationContext(context);
            //var scheduler = new SingleThreadedSynchronizationContextTaskScheduler(context);

#if FULL_GPIO
            Logic = new Logic<Light>(TaskScheduler.Default, il => new Light(), ol => new Light());
#else
            Logic = new Logic<Light>(TaskScheduler.Default, il => new Light(), ol => new Light());
            var iMapping = new int[] { 5, 6, 13, 19, 26 };
            var oMapping = new int[] { 12, 20, 21 };
#endif

            InputPinCount = Logic.Inputs.Length;
            OutputPinCount = Logic.Outputs.Length;

            var lastOutputs = new bool[OutputPinCount];
            var firstIteration = true;

            var gpio = new GpioController();

            LogicInitialized?.Set();

            for (int i = 0; i < InputPinCount; ++i) gpio.OpenPin(iMapping[i], PinMode.Input);
            for (int i = 0; i < OutputPinCount; ++i) if (i < 3) gpio.OpenPin(oMapping[i], PinMode.Output);
            //gpio.OpenPin(ResetPin, PinMode.Input);

            Stopwatch.Start();
            while (true)
            {
                //if (gpio.Read(ResetPin) == PinValue.High) Logic.Reset();

                // read the new inputs
                for (int i = 0; i < InputPinCount; ++i)
                    Logic.Inputs[i].Active = gpio.Read(iMapping[i]) == PinValue.High;

                var elapsed = Stopwatch.Elapsed;
                Logic.Process(elapsed);

                // execute all queued actions
                //context.ExecuteAllWorkItems();

                // write the outputs
                if (firstIteration || !lastOutputs.SequenceEqual(Logic.Outputs.Select(w => w.Active)))
                {
                    Logic.Outputs.Where((w, idx) => idx < 3).ForEach((w, idx) =>
                        gpio.Write(oMapping[idx],
                            idx >= 3 ?
                                w.Active ? PinValue.High : PinValue.Low :
                                w.Active ? PinValue.Low : PinValue.High));

                    firstIteration = false;

                    for (int i = 0; i < OutputPinCount; ++i)
                        lastOutputs[i] = Logic.Outputs[i].Active;
                }

                ++Cycles;

                if (elapsed - LastCycleEnd >= CycleInterval)
                {
                    Console.WriteLine($"Cycle rate: {Cycles / (elapsed - LastCycleEnd).TotalSeconds / 1000d:0.00} kHz");
                    LastCycleEnd = elapsed;
                    Cycles = 0;
                }
            }
        }

        public class Light : ILight
        {
            public bool Active { get; set; }
            public string Text { get; set; }
        }
    }
}
