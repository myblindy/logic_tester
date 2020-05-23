using Avalonia.Media;
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
            new LightModel { Type = LightType.Input, Index = 0, ActiveBackColor = Colors.Blue, ActiveColor = Colors.White, InactiveBackColor = Colors.LightBlue, InactiveColor = Colors.Black, Text = "Close Limit" },
            new LightModel { Type = LightType.Input, Index = 1, ActiveBackColor = Colors.Blue, ActiveColor = Colors.White, InactiveBackColor = Colors.LightBlue, InactiveColor = Colors.Black, Text = "Advanced Close Limit" },
            new LightModel { Type = LightType.Input, Index = 2, ActiveBackColor = Colors.Blue, ActiveColor = Colors.White, InactiveBackColor = Colors.LightBlue, InactiveColor = Colors.Black, Text = "Advanced Open Limit" },
            new LightModel { Type = LightType.Input, Index = 3, ActiveBackColor = Colors.Blue, ActiveColor = Colors.White, InactiveBackColor = Colors.LightBlue, InactiveColor = Colors.Black, Text = "Open Limit" },

            new LightModel { Type = LightType.Output, Index = 0, ActiveBackColor = Colors.Gray, ActiveColor = Colors.Black, InactiveBackColor = Colors.LightGray, InactiveColor = Colors.Black, Text = "Open Button" },
            new LightModel { Type = LightType.Output, Index = 1, ActiveBackColor = Colors.Gray, ActiveColor = Colors.Black, InactiveBackColor = Colors.LightGray, InactiveColor = Colors.Black, Text = "Close Button" },
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
            if (!(Logic is null))
            {
                Lights.ForEach(l => l.Active = ((ILight)(l.Type == LightType.Input ? Logic.Inputs[l.Index] : Logic.Outputs[l.Index])).Active);
                Counter = Logic.Counter;
                State = Logic.State;
                Region = Logic.Region;
            }
        }
    }
}
