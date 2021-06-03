using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WPF_GalacticRunSolver.Model;

namespace WPF_GalacticRunSolver.Converters
{
    [ValueConversion(typeof(EColor), typeof(BitmapImage))]
    public class RobotColorToImageConverter : IValueConverter
    {
        public static RobotColorToImageConverter Instance = new RobotColorToImageConverter();

        private readonly Dictionary<EColor, string> _RobotColorToPathDictionary = new Dictionary<EColor, string>
        {
            {EColor.Red, "Images/Red.png" },
            {EColor.Blue, "Images/Blue.png" },
            {EColor.Green, "Images/Green.png" },
            {EColor.Yellow, "Images/Yellow.png" },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (EColor)value;
            var image = _RobotColorToPathDictionary[type];
            return new BitmapImage(new Uri($"pack://application:,,,/{image}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}