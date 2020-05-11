using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace tester_rpi_LCD.Conveters
{
    class PinStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool active = (bool)value;
            if (active)
                return new SolidColorBrush(Colors.Blue);
            return new SolidColorBrush(Colors.LightBlue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
