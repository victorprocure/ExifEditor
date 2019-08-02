using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace ProcureSoft.ExifEditor.Infrastructure.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public sealed class FileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string valueString))
                return string.Empty;

            var lowerParams = parameter?.ToString()?.ToLower();
            var withExtension = true;
            switch (lowerParams)
            {
                case "":
                case "with":
                case null:
                    withExtension = true;
                    break;

                case "without":
                    withExtension = false;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(parameter));
            }

            return GetFileName(valueString, !withExtension);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => null;

        private static string GetFileName(string path, bool withoutExtension = false)
        {
            if (string.IsNullOrEmpty(path) || path.EndsWith(@":\") || path.Equals(@"\") || path.Equals("/"))
                return path;

            return !withoutExtension ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path);
        }
    }
}