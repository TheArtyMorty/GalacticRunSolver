using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WPF_GalacticRunSolver.Model;

namespace WPF_GalacticRunSolver.Converters
{
    [ValueConversion(typeof(CLI.ERobotColor), typeof(BitmapImage))]
    public class MoveToImageConverter : IMultiValueConverter  
    {
        public static MoveToImageConverter Instance = new MoveToImageConverter();

        private readonly Dictionary<CLI.ERobotColor, string> _TargetColorToPathDictionary = new Dictionary<CLI.ERobotColor, string>
        {
            {CLI.ERobotColor.Red, "Images/ArrowRed.png" },
            {CLI.ERobotColor.Blue, "Images/ArrowBlue.png" },
            {CLI.ERobotColor.Green, "Images/ArrowGreen.png" },
            {CLI.ERobotColor.Yellow, "Images/ArrowYellow.png" },
            {CLI.ERobotColor.Gray, "Images/ArrowGray.png" },
        };

        private Rotation GetImageRotationFromDirection(CLI.EMoveDirection direction)
        {
            switch (direction)
            {
                case CLI.EMoveDirection.Right:
                    return Rotation.Rotate90;
                case CLI.EMoveDirection.Down:
                    return Rotation.Rotate180;
                case CLI.EMoveDirection.Left:
                    return Rotation.Rotate270;
                case CLI.EMoveDirection.Up:
                default:
                    return Rotation.Rotate0;
            }
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (CLI.ERobotColor)values[0];
            var image = _TargetColorToPathDictionary[color];
            var result = new BitmapImage();
            result.BeginInit();
            result.UriSource = new Uri($"pack://application:,,,/{image}");
            var direction = (CLI.EMoveDirection)values[1];
            result.Rotation = GetImageRotationFromDirection(direction);
            result.EndInit();
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
