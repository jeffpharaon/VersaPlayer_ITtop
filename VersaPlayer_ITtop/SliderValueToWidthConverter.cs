using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VersaPlayer_ITtop
{
    public class SliderValueToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double sliderValue)
            {
                if (parameter != null && parameter.ToString() == "Thumb") return new Thickness(sliderValue * 3.84, 0, 0, 0);
                return sliderValue * 3.84;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

