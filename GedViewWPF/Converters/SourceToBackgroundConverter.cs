using System;
using System.Windows.Data;
using System.Windows.Media;
using GedViewWPF.Model;
using GedViewWPF.DataAccess;
using GedViewWPF.Utilities;
using System.Windows;

namespace GedViewWPF.Converters
{
    class SourceToBackgroundConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var colorStart = Colors.White;
			var colorEnd = Colors.LightGray;
			var clue = (Clue) value;

			switch ((string)parameter)
			{
				case "Ssdi":
					if (!string.IsNullOrEmpty(clue.Ssdi))
						colorEnd = IsSourced(clue.Person, "Social Security Death Index") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "Grave":
					if (!string.IsNullOrEmpty(clue.Grave))
						colorEnd = IsSourced(clue.Person, "Find A Grave") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "1850":
					if (!string.IsNullOrEmpty(clue.Census1850))
						colorEnd = IsSourced(clue.Person, "1850 United States Federal Census") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "1860":
					if (!string.IsNullOrEmpty(clue.Census1860))
						colorEnd = IsSourced(clue.Person, "1860 United States Federal Census") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "1870":
					if (!string.IsNullOrEmpty(clue.Census1870))
						colorEnd = IsSourced(clue.Person, "1870 United States Federal Census") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "1880":
					if (!string.IsNullOrEmpty(clue.Census1880))
						colorEnd = IsSourced(clue.Person, "1880 United States Federal Census") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "1890":
					if (!string.IsNullOrEmpty(clue.Census1890))
						colorEnd = IsSourced(clue.Person, "1890 United States Federal Census") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "1900":
					if (!string.IsNullOrEmpty(clue.Census1900))
						colorEnd = IsSourced(clue.Person, "1900 United States Federal Census") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "1910":
					if (!string.IsNullOrEmpty(clue.Census1910))
						colorEnd = IsSourced(clue.Person, "1910 United States Federal Census") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "1920":
					if (!string.IsNullOrEmpty(clue.Census1920))
						colorEnd = IsSourced(clue.Person, "1920 United States Federal Census") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "1930":
					if (!string.IsNullOrEmpty(clue.Census1930))
						colorEnd = IsSourced(clue.Person, "1930 United States Federal Census") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "1940":
					if (!string.IsNullOrEmpty(clue.Census1930))
						colorEnd = IsSourced(clue.Person, "1940 United States Federal Census") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "CivilWar":
					if (!string.IsNullOrEmpty(clue.MilitaryCivil))
						colorEnd = IsSourced(clue.Person, "U.S. Civil War") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "WWI":
					if (!string.IsNullOrEmpty(clue.MilitaryWWI))
						colorEnd = IsSourced(clue.Person, "World War I") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "WWII":
					if (!string.IsNullOrEmpty(clue.MilitaryWWII))
						colorEnd = IsSourced(clue.Person, "World War II") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "Korea":
					if (!string.IsNullOrEmpty(clue.MilitaryKorea))
						colorEnd = IsSourced(clue.Person, "Korean") ? Colors.LightGreen : Colors.LightSalmon;
					break;
				case "Vietnam":
					if (!string.IsNullOrEmpty(clue.MilitaryVietnam))
						colorEnd = IsSourced(clue.Person, "Vietnam") ? Colors.LightGreen : Colors.LightSalmon;
					break;

				default:
					break;

			}

			return new LinearGradientBrush(colorStart, colorEnd, new Point(0,.67), new Point(0,1));
		}

		private bool IsSourced(Person person, string title)
		{
			DataManager dataMgr = Utils.GetDataManager();
			foreach (var fact in person.Facts)
			{
				foreach (var citation in dataMgr.GetCitations())
				{
					if (citation.FactIDs.Contains(fact.ID))
					{
						var source = dataMgr.GetSourceByID(citation.SourceID);

						if ((source != null) && source.Title.StartsWith(title))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
    }
}
