using SolverApp.Models;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace SolverApp.Converters
{
    public class MoveDirectionToRotationConverter : IValueConverter
    {
        public static MoveDirectionToRotationConverter Instance = new MoveDirectionToRotationConverter();

        private double GetImageRotationFromDirection(EMoveDirection direction)
        {
            switch (direction)
            {
                case EMoveDirection.Right:
                    return 90;
                case EMoveDirection.Down:
                    return 180;
                case EMoveDirection.Left:
                    return 270;
                case EMoveDirection.Up:
                default:
                    return 0;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var direction = (EMoveDirection)value;
            return GetImageRotationFromDirection(direction);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
