using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WPF_GalacticRunSolver.Model;

namespace WPF_GalacticRunSolver.Converters
{
    [ValueConversion(typeof(EColor), typeof(BitmapImage))]
    public class TargetColorToImageConverter : IValueConverter
    {
        public static TargetColorToImageConverter Instance = new TargetColorToImageConverter();

        private readonly Dictionary<EColor, string> _TargetColorToPathDictionary = new Dictionary<EColor, string>
        {
            {EColor.Red, "Images/RedTarget.png" },
            {EColor.Blue, "Images/BlueTarget.png" },
            {EColor.Green, "Images/GreenTarget.png" },
            {EColor.Yellow, "Images/YellowTarget.png" },
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
