using SolverApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;

namespace SolverApp.Converters
{
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
            return _TargetColorToPathDictionary[type];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
