using Avalonia.Media;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace tester_rpi_LCD.Model
{
    enum LightType
    {
        Input, Output
    }

    class LightModel : ReactiveObject
    {
        private bool active;
        public bool Active { get => active; set => this.RaiseAndSetIfChanged(ref active, value); }

        private string text;
        public string Text { get => text; set => this.RaiseAndSetIfChanged(ref text, value); }

        private LightType type;
        public LightType Type { get => type; set => this.RaiseAndSetIfChanged(ref type, value); }

        private int index;
        public int Index { get => index; set => this.RaiseAndSetIfChanged(ref index, value); }

        private Color activeBackColor, inactiveBackColor;
        public Color ActiveBackColor { get => activeBackColor; set => this.RaiseAndSetIfChanged(ref activeBackColor, value); }
        public Color InactiveBackColor { get => inactiveBackColor; set => this.RaiseAndSetIfChanged(ref inactiveBackColor, value); }

        private Color activeColor, inactiveColor;
        public Color ActiveColor { get => activeColor; set => this.RaiseAndSetIfChanged(ref activeColor, value); }
        public Color InactiveColor { get => inactiveColor; set => this.RaiseAndSetIfChanged(ref inactiveColor, value); }
    }
}
