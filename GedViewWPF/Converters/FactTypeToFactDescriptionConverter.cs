using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using GedcomReader;

namespace GedViewWPF.Converters
{
    class FactTypeToFactDescriptionConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            var factType = (string)value;

            if (value.Equals("BIRT"))
                return "Birth";

            if (value.Equals("MARR"))
                return "Marriage";

            if (value.Equals("DEAT"))
                return "Death";

            if (value.Equals("BURI"))
                return "Burial";

            if (value.Equals("GRAD"))
                return "Graduation";

            if (value.Equals("RESI"))
                return "Residence";

            if (value.Equals("SSN"))
                return "Social Security Number";

            if (value.Equals("CONL"))
                return "Confirmation (LDS)";

            if (value.Equals("BAPL"))
                return "Baptism (LDS)";

            if (value.Equals("ENDL"))
                return "Endowment (LDS)";

            if (value.Equals("SLGC"))
                return "Sealing to Parents (LDS)";

            if (value.Equals("SLGS"))
                return "Sealing to Spouse (LDS)";


            return factType;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            return null;
		}
    }
}
