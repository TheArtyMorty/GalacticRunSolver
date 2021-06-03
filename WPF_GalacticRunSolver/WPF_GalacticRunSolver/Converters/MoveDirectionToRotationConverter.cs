using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WPF_GalacticRunSolver.Model;

namespace WPF_GalacticRunSolver.Converters
{
    [ValueConversion(typeof(CLI.EMoveDirection), typeof(BitmapImage))]
    public class MoveDirectionToRotationConverter : IValueConverter
    {
        public static MoveDirectionToRotationConverter Instance = new MoveDirectionToRotationConverter();

        private readonly Dictionary<EColor, string> _TargetColorToPathDictionary = new Dictionary<EColor, string>
        {
            {EColor.Red, "Images/ArrowRed.png" },
            {EColor.Blue, "Images/ArrowBlue.png" },
            {EColor.Green, "Images/ArrowGreen.png" },
            {EColor.Yellow, "Images/ArrowYellow.png" },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (EColor)value;
            var image = _TargetColorToPathDictionary[type];
            return new BitmapImage(new Uri($"pack://application:,,,/{image}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
