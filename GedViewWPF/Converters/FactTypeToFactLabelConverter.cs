using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using GedcomReader;

namespace GedViewWPF.Converters
{
    class FactTypeToFactLabelConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            var factType = (string)value;
            var factDesc = "[unknown]";

            if (value.Equals(Gedcom.Tokens.Birth.ToString()))
                factDesc = "Birth";
            if (value.Equals(Gedcom.Tokens.Marriage.ToString()))
                factDesc = "Marr";
            if (value.Equals(Gedcom.Tokens.Death.ToString()))
                factDesc = "Death";
            if (value.Equals(Gedcom.Tokens.Burial.ToString()))
                factDesc = "Burial";
            if (value.Equals(Gedcom.Tokens.BaptismLDS.ToString()))
                factDesc = "BapLDS";
            if (value.Equals(Gedcom.Tokens.ConfirmationLDS.ToString()))
                factDesc = "ConfLDS";
            if (value.Equals(Gedcom.Tokens.EndowmentLDS.ToString()))
                factDesc = "EndowLDS";
            if (value.Equals(Gedcom.Tokens.SealParentLDS.ToString()))
                factDesc = "SealParLDS";
            if (value.Equals(Gedcom.Tokens.SealSpouseLDS.ToString()))
                factDesc = "SealSpLDS";

            return factDesc;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            return null;
		}
    }
}
