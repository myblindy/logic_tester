using Avalonia.Media;
using logic;
using MoreLinq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;

namespace tester_rpi_LCD.Model
{
    class MainModel : ReactiveObject
    {
        public dynamic Logic { get; set; }

        public LightModel[] Lights { get; } = new LightModel[]
        {
            new LightModel { Type = LightType.Output, Index = 3, ActiveBackColor = Colors.Blue, ActiveColor = Colors.White, InactiveBackColor = Colors.LightBlue, InactiveColor = Colors.Black, Text = "Close Limit" },
            new LightModel { Type = LightType.Output, Index = 4, ActiveBackColor = Colors.Blue, ActiveColor = Colors.White, InactiveBackColor = Colors.LightBlue, InactiveColor = Colors.Black, Text = "Adv. Close Limit" },
            new LightModel { Type = LightType.Output, Index = 5, ActiveBackColor = Colors.Blue, ActiveColor = Colors.White, InactiveBackColor = Colors.LightBlue, InactiveColor = Colors.Black, Text = "Adv. Open Limit" },
            new LightModel { Type = LightType.Output, Index = 6, ActiveBackColor = Colors.Blue, ActiveColor = Colors.White, InactiveBackColor = Colors.LightBlue, InactiveColor = Colors.Black, Text = "Open Limit" },

            new LightModel { Type = LightType.Output, Index = 0, ActiveBackColor = Colors.Black, ActiveColor = Colors.White, InactiveBackColor = Colors.LightGray, InactiveColor = Colors.Black, Text = "Open Button" },
            new LightModel { Type = LightType.Output, Index = 1, ActiveBackColor = Colors.Black, ActiveColor = Colors.White, InactiveBackColor = Colors.LightGray, InactiveColor = Colors.Black, Text = "Close Button" },
        };

        private int counter;
        public int Counter { get => counter; set => this.RaiseAndSetIfChanged(ref counter, value); }

        private int state;
        public int State { get => state; set => this.RaiseAndSetIfChanged(ref state, value); }

        private int region;
        public int Region { get => region; set => this.RaiseAndSetIfChanged(ref region, value); }

        public void ResetCommand()
        {
            Logic?.Reset();
        }

        public void UpdateLiveValues()
        {
            if (Logic is ILogic logic)
            {
                Lights.ForEach(l => l.Active = ((ILight)(l.Type == LightType.Input ? Logic.Inputs[l.Index] : Logic.Outputs[l.Index])).Active);
                Counter = logic.Counter;
                State = logic.State;
                Region = logic.Region;
            }
        }
    }
}
