using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace tester_rpi_LCD.Model
{
    class MainModel : ReactiveObject
    {
        public LightModel[] Lights { get; } = new LightModel[]
        {
            new LightModel { Text = "Close Limit", Active = true },
            new LightModel { Text = "Advanced Close Limit" },
            new LightModel { Text = "Advanced Open Limit" },
            new LightModel { Text = "Open Limit" },
        };

        private int counter;
        public int Counter { get => counter; set => this.RaiseAndSetIfChanged(ref counter, value); }

        private int state;
        public int State { get => state; set => this.RaiseAndSetIfChanged(ref state, value); }

        private int region;
        public int Region { get => region; set => this.RaiseAndSetIfChanged(ref region, value); }
    }
}
