using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;

namespace GedViewWPF.Converters
{
    class FilePathToFilenameConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            var path = value as string;
            var filename = Path.GetFileName(path);

            return filename;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            return null;
		}
    }
}
