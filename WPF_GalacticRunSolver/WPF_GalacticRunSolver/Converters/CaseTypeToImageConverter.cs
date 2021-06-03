using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WPF_GalacticRunSolver.Model;

namespace WPF_GalacticRunSolver.Converters
{
    [ValueConversion(typeof(EWallType), typeof(BitmapImage))]
    public class CaseTypeToImageConverter : IValueConverter
    {
        public static CaseTypeToImageConverter Instance = new CaseTypeToImageConverter();

        private readonly Dictionary<EWallType, string> _WallTypeToPathDictionary = new Dictionary<EWallType, string>
        {
            {EWallType.None, "Images/Empty.png" },
            {EWallType.TopLeft, "Images/TopLeft.png" },
            {EWallType.TopRight, "Images/TopRight.png" },
            {EWallType.BottomLeft, "Images/BottomLeft.png" },
            {EWallType.BottomRight, "Images/BottomRight.png" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (EWallType)value;
            var image = _WallTypeToPathDictionary[type];
            return new BitmapImage(new Uri($"pack://application:,,,/{image}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
