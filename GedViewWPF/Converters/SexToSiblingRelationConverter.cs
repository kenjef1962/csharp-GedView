using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GedViewWPF.Converters
{
    class SexToSiblingRelationConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            var relation = "<unknown>";

            if (value is string)
            {
                var sex = value as string;
                switch (sex.ToUpper())
                {
                    case "M":
                        relation = "brother";
                        break;

                    case "F":
                        relation = "sister";
                        break;
                }
            }

            return relation;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            return null;
		}
    }
}
