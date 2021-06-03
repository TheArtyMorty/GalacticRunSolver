using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WPF_GalacticRunSolver.Model;

namespace WPF_GalacticRunSolver.Converters
{
    [ValueConversion(typeof(Position), typeof(Thickness))]
    public class PositionToMarginConverter : IValueConverter
    {
        public static PositionToMarginConverter Instance = new PositionToMarginConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var position = (Position)value;
            var margin = new Thickness(position.X * App.CellSize, position.Y * App.CellSize, 0, 0);
            return margin;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
