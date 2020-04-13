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
        private const int InputPinCount = 5;
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

            for (int i = 0; i < InputPinCount; ++i) gpio.OpenPin(i, PinMode.Input);
            for (int i = 0; i < OutputPinCount; ++i) gpio.OpenPin(i + InputPinCount, PinMode.Output);
            gpio.OpenPin(ResetPin, PinMode.Input);

            var inputpairs = Enumerable.Range(0, InputPinCount).Select(idx => new PinValuePair(idx, PinValue.Low)).ToArray();
            var outputpairs = Enumerable.Range(0, OutputPinCount).Select(idx => new PinValuePair(idx, PinValue.Low)).ToArray();

            Stopwatch.Start();
            while (true)
            {
                gpio.Read(inputpairs);
                if (inputpairs[4].PinValue == PinValue.High) logic.Reset();

                // read the new inputs
                for (int i = 0; i < InputPinCount; ++i)
                    inputs[i].Active = inputpairs[i].PinValue == PinValue.High;

                logic.Process();

                // write the outputs
                outputs.ForEach((w, idx) => outputpairs[idx] = new PinValuePair(idx + InputPinCount, w.Active ? PinValue.High : PinValue.Low));
                gpio.Write(outputpairs);

                ++Cycles;

                var elapsed = Stopwatch.Elapsed;
                if (elapsed - LastCycleEnd >= CycleInterval)
                {
                    Console.WriteLine($"Cycle rate: {Cycles / (elapsed - LastCycleEnd).TotalSeconds / 1000d:0.00} kHz");
                    LastCycleEnd = elapsed;
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
