using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using GedViewWPF.Utilities;

namespace GedViewWPF.Converters
{
    class ImagePathToThumbnailConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Utils.CreateThumbnailImageSource(value as string);
        }

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            return null;
		}
    }
}
