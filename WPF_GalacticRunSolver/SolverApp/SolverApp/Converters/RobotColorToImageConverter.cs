using SolverApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;

namespace SolverApp.Converters
{
    public class RobotColorToImageConverter : IValueConverter
    {
        public static RobotColorToImageConverter Instance = new RobotColorToImageConverter();

        private readonly Dictionary<EColor, string> _RobotColorToPathDictionary = new Dictionary<EColor, string>
        {
            {EColor.Red, "Images/Red.png" },
            {EColor.Blue, "Images/Blue.png" },
            {EColor.Green, "Images/Green.png" },
            {EColor.Yellow, "Images/Yellow.png" },
            {EColor.Gray, "Images/Gray.png" },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (EColor)value;
            return _RobotColorToPathDictionary[type];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}