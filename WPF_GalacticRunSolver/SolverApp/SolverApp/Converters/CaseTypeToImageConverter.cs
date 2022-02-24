using SolverApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;

namespace SolverApp.Converters
{
    public class CaseTypeToImageConverter : IValueConverter
    {
        public static CaseTypeToImageConverter Instance = new CaseTypeToImageConverter();

        private readonly Dictionary<EWallType, string> _WallTypeToPathDictionary = new Dictionary<EWallType, string>
        {
            {EWallType.None, "Empty.png" },
            {EWallType.TopLeft, "TopLeft.png" },
            {EWallType.TopRight, "TopRight.png" },
            {EWallType.BottomLeft, "BottomLeft.png" },
            {EWallType.BottomRight, "BottomRight.png" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (EWallType)value;
            return _WallTypeToPathDictionary[type];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
