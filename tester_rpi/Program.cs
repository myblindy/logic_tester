using System;
using System.Linq;
using System.Device.Gpio;

namespace tester_rpi
{
    class Program
    {
        private const int ResetPin = 27;
        private const int InputPinCount = 5;
        private const int OutputPinCount = 16;

        static void Main(string[] _)
        {
            var inputs = Enumerable.Range(0, InputPinCount).Select(idx => new Light()).ToArray();
            var outputs = Enumerable.Range(0, OutputPinCount).Select(idx => new Light()).ToArray();

            var logic = new Logic(inputs, outputs);
            var gpio = new GpioController();

            for (int i = 0; i < InputPinCount; ++i) gpio.OpenPin(i, PinMode.Input);
            for (int i = 0; i < OutputPinCount; ++i) gpio.OpenPin(i + InputPinCount, PinMode.Output);
            gpio.OpenPin(ResetPin, PinMode.Input);

            while (true)
            {
                if (gpio.Read(ResetPin) == PinValue.High) logic.Reset();

                // read the new inputs
                for (int i = 0; i < InputPinCount; ++i)
                    inputs[i].Active = gpio.Read(i) == PinValue.High;

                logic.Process();

                // write the outputs
                gpio.Write(Enumerable.Range(0, OutputPinCount).Select(idx => new PinValuePair(idx + InputPinCount, outputs[idx].Active)).ToArray());
            }
        }

        internal class Light : ILight
        {
            public bool Active { get; set; }
            public string Text { get; set; }
        }
    }
}
