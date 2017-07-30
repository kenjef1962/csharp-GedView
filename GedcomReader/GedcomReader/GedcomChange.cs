using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomChange : GedcomEntity
    {
        public string Date { get; set; }
        public string Time { get; set; }

		public bool IsValid()
		{
			if (string.IsNullOrEmpty(Date))
				return false;

			return true;
		}

		public static GedcomChange Read(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
			line = GedcomLine.Read(stream);

			if (!ReadToken.CanReadToken(stream, lineIn, "CHAN"))
				return null;

            var gedcomChange = new GedcomChange();

            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case "DATE":	// <CHANGE_DATE>, required
						ValidateTokenData.IsDuplicate(line, gedcomChange.Date);
						ValidateTokenData.IsValidString(line, 1, 40);
						gedcomChange.Date = line.Data;
						line = ReadToken_DATE(stream, line, gedcomChange);
                        break;

                    case "NOTE":	// <NOTE_CHANGE>
						line = ReadToken.ReadNote(stream, line, gedcomChange);
                        break;

                    default:
                        line = ReadToken.ReadUnsupportedToken(stream, line);
                        break;
                }
            }

            return gedcomChange;
        }

		private static GedcomLine ReadToken_DATE(StreamReader stream, GedcomLine lineIn, GedcomChange gedcomChange)
		{
			var line = GedcomLine.Read(stream);

			if (!ReadToken.CanReadToken(stream, lineIn, "DATE"))
				return line;

			while ((line != null) && (lineIn.Level < line.Level))
			{
				switch (line.Token)
				{
					case "TIME":	// <TIME_VALUE>
						ValidateTokenData.IsDuplicate(line, gedcomChange.Time);
						ValidateTokenData.IsValidString(line, 1, 40);
						gedcomChange.Time = line.Data;
						line = GedcomLine.Read(stream);
						break;

					default:
						line = ReadToken.ReadUnsupportedToken(stream, line);
						break;
				}
			}

			return line;
		}
	}
}
