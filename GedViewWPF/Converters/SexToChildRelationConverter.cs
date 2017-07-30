using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GedViewWPF.Converters
{
    class SexToChildRelationConverter : IValueConverter
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
                        relation = "son";
                        break;

                    case "F":
                        relation = "daughter";
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
