using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GedViewWPF.Converters
{
    class SexToSilhouetteImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage silhouette = new BitmapImage();
            silhouette.BeginInit();

            string sex = value as string;
            if (sex.Equals("M"))
				silhouette.UriSource = new Uri("file:///C:/Temp/GedViewWPF/GedViewWPF/Resources/silhouette_male.png");
			else
				silhouette.UriSource = new Uri("file:///C:/Temp/GedViewWPF/GedViewWPF/Resources/silhouette_female.png");

            silhouette.EndInit();
            return silhouette;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
