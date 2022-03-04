using SolverApp.Models;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace SolverApp.Converters
{
    public class PositionToLayoutBoundsConverter : IValueConverter
    {
        public static PositionToLayoutBoundsConverter Instance = new PositionToLayoutBoundsConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var position = (Position)value;
            int x = position.X * (App.CellSize + App.CellSpacing);
            int y = position.Y * (App.CellSize + App.CellSpacing);
            var layoutBounds = new Rectangle(x, y, App.CellSize, App.CellSize);
            return layoutBounds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
