using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace tester_rpi_LCD.Model
{
    class LightModel : ReactiveObject
    {
        private bool active;
        public bool Active { get => active; set => this.RaiseAndSetIfChanged(ref active, value); }

        private string text;
        public string Text { get => text; set => this.RaiseAndSetIfChanged(ref text, value); }
    }
}
