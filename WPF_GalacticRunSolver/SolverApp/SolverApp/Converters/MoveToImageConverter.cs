using SolverApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;

namespace SolverApp.Converters
{
    public class MoveToImageConverter : IValueConverter
    {
        public static MoveToImageConverter Instance = new MoveToImageConverter();

        private readonly Dictionary<EColor, string> _TargetColorToPathDictionary = new Dictionary<EColor, string>
        {
            {EColor.Red, "ArrowRed.png" },
            {EColor.Blue, "ArrowBlue.png" },
            {EColor.Green, "ArrowGreen.png" },
            {EColor.Yellow, "ArrowYellow.png" },
            {EColor.Gray, "ArrowGray.png" },
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
