using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace tester_rpi_LCD.Conveters
{
    class PinStateToColorConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture) =>
            !values.OfType<BindingNotification>().Any() ? new SolidColorBrush((Color)values[(bool)values[0] ? 1 : 2]) : null;
    }
}
