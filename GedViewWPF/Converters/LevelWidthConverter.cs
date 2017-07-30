using System;
using System.Globalization;
using System.Windows.Data;

namespace GedViewWPF.Converters
{
	class LevelWidthConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return 115 - ((int)value * 19);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
