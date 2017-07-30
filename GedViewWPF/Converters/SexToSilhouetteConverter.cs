using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GedViewWPF.Converters
{
    class SexToSilhouetteConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            var silhouette = Properties.Resources.SilhouetteUnknown;

            if (value is string)
            {
                var sex = value as string;
                switch (sex.ToUpper())
                {
                    case "M":
                        silhouette = Properties.Resources.SilhouetteMale;
                        break;

                    case "F":
                        silhouette = Properties.Resources.SilhouetteFemale;
                        break;
                }
            }

            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(silhouette.GetHbitmap(),
                                                                                IntPtr.Zero,
                                                                                Int32Rect.Empty,
                                                                                BitmapSizeOptions.FromEmptyOptions());
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            return null;
		}
    }
}
