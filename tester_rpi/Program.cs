using System;
using System.Linq;
using System.Device.Gpio;
using System.Diagnostics;
using MoreLinq;

namespace tester_rpi
{
    class Program
    {
        private const int ResetPin = 27;

        private const int InputPin0 = 22;
        private const int InputPinCount = 5;

        private const int OutputPin0 = 5;
        private const int OutputPinCount = 16;

        static readonly Stopwatch Stopwatch = new Stopwatch();
        static int Cycles;
        static readonly TimeSpan CycleInterval = TimeSpan.FromSeconds(5);
        static TimeSpan LastCycleEnd = TimeSpan.Zero;

        static void Main(string[] _)
        {
            var inputs = Enumerable.Range(0, InputPinCount).Select(idx => new Light()).ToArray();
            var outputs = Enumerable.Range(0, OutputPinCount).Select(idx => new Light()).ToArray();

            var logic = new Logic(inputs, outputs);
            var gpio = new GpioController();

            for (int i = 0; i < InputPinCount; ++i) gpio.OpenPin(i + InputPin0, PinMode.Input);
            for (int i = 0; i < OutputPinCount; ++i) gpio.OpenPin(i + OutputPin0, PinMode.Output);
            gpio.OpenPin(ResetPin, PinMode.Input);

            Stopwatch.Start();
            while (true)
            {
                //gpio.Read(inputpairs);
                if (gpio.Read(ResetPin) == PinValue.High) logic.Reset();

                // read the new inputs
                for (int i = 0; i < InputPinCount; ++i)
                    inputs[i].Active = gpio.Read(i + InputPin0) == PinValue.High;

                var elapsed = Stopwatch.Elapsed;
                logic.Process(elapsed);

                // write the outputs
                outputs.ForEach((w, idx) => gpio.Write(idx + OutputPin0,
                    idx >= 3 ?
                        w.Active ? PinValue.High : PinValue.Low :
                        w.Active ? PinValue.Low : PinValue.High));

                ++Cycles;

                if (elapsed - LastCycleEnd >= CycleInterval)
                {
                    Console.WriteLine($"Cycle rate: {Cycles / (elapsed - LastCycleEnd).TotalSeconds / 1000d:0.00} kHz");
                    LastCycleEnd = elapsed;
                    Cycles = 0;
                }
            }
        }

        internal class Light : ILight
        {
            public bool Active { get; set; }
            public string Text { get; set; }
        }
    }
}
