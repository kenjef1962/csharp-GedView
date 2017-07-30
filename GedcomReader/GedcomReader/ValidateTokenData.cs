using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
	public class ValidateTokenData
	{
		internal static List<string> ValidIndividualFacts = new List<string>
        {
            // Attributes
            "CAST", "DSCR", "EDUC", "IDNO", "NATI", "NCHI", "NMR", "OCCU",
            "PROP", "RELI", "RESI", "SSN", "TITL",

            // Events
            "BIRT", "CHR", "DEAT", "BURI", "CREM", "ADOP", "BAPM", "BARM", 
            "BASM", "BLES", "CHRA", "CONF", "FCOM", "ORDN", "NATU", "EMIG",
            "IMMI", "CENS", "PROB", "WILL", "GRAD", "RETI",

			// FTM Extensions
			"ALIA", "_MILT", "_MISN"
        };
		internal static List<string> ValidIndividualFactsLDS = new List<string>
        {
            "BAPL", "CONL", "ENDL", "SLGC",
        };
		internal static List<string> ValidFamilyFacts = new List<string>
        {
            "ANUL", "CENS", "DIV", "DIVF", "ENGA", "MARR", "MARB", "MARC", 
            "MARL", "MARS"
        };
		internal static List<string> ValidFamilyFactsLDS = new List<string>
        {
            "SLGS",
        };
		internal static List<string> ValidCustomFacts = new List<string>
        {
            "EVEN",
        };
		internal static List<string> ValidSexValues = new List<string>
        {
            "M", "MALE", "F", "FEMALE", "U", "UNKNOWN"
        };
		internal static List<string> ValidOrdinanceStatusLDS = new List<string>
        {
            "BIC", "CANCELED", "CHILD", "CLEARED", "COMPLETED", "DNS", 
			"INFANT", "QUALIFIED", "PRE-1970", "STILLBORN", "SUBMITTED", "UNCLEARED"
        };


		internal static bool IsDuplicate(GedcomLine line, object currentValue)
		{
			var message = string.Empty;

			if (currentValue is string)
			{
				var value = (string)currentValue;
				
				if (!string.IsNullOrEmpty(value))
					message = "Previous value already exists";
			}
			else if (currentValue is List<string>)
			{
				var value = (List<string>)currentValue;

				if (value.Contains(currentValue))
					message = "Previous value already in list";
			}
			else
			{
				if (currentValue != null)
					message = "Previous value already in list";
			}
			if (!string.IsNullOrEmpty(message))
			{
				Gedcom.WriteLogItem(message, line);
				return false;
			}

			return true;
		}

		internal static bool IsValidNumber(GedcomLine line, int minValue, int maxValue)
		{
			var message = string.Empty;

			int val;
			if (!int.TryParse(line.Data, out val))
				message = "Not a valid value";

			else if ((0 < minValue) && (val < minValue))
				message = string.Format("Value length less than {0}", minValue);

			else if ((0 < maxValue) && (maxValue < val))
				message = string.Format("Value length greater than {0}", maxValue);

			if (!string.IsNullOrEmpty(message))
			{
				Gedcom.WriteLogItem(message, line);
				return false;
			}

			return true;
		}

		internal static bool IsValidString(GedcomLine line, int minLength, int maxLength)
		{
			var message = string.Empty;

			if ((0 < minLength) && string.IsNullOrEmpty(line.Data))
				message = "Missing value";

			else if ((0 < minLength) && (line.Data.Length < minLength))
				message = string.Format("Value length less than {0}", minLength);

			else if ((0 < maxLength) && (maxLength < line.Data.Length))
				message = string.Format("Value length greater than {0}", maxLength);

			if (!string.IsNullOrEmpty(message))
			{
				Gedcom.WriteLogItem(message, line);
				return false;
			}

			return true;
		}

		internal static bool IsValidXref(GedcomLine line)
		{
			var message = string.Empty;

			if (string.IsNullOrEmpty(line.Data))
				message = "Missing value";

			if (line.Data.Length < 1)
				message = "Value length less than 1";

			if (22 < line.Data.Length)
				message = "Value greater than than 22";

			if (!line.Data.StartsWith("@") || !line.Data.EndsWith("@"))
				message = "Value needs to start/end with '@'";

			if (!string.IsNullOrEmpty(message))
			{
				Gedcom.WriteLogItem(message, line);
				return false;
			}

			return true;
		}

		internal static bool LimitedValues(GedcomLine line, List<string> values)
		{
			if (!values.Contains(line.Data))
				return false;

			return true;
		}
	}
}
