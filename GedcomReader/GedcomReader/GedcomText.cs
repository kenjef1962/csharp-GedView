using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomText
    {
        public static string Read(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
            line = GedcomLine.Read(stream);

            if ((stream == null) || (lineIn == null))
                return null;

            var text = lineIn.Data;
            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case "TEXT":	// <SUBMITTER_TEXT>
						ValidateTokenData.IsValidString(line, 0, 248);
                        text = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "CONC":	// <SUBMITTER_TEXT>
					case "CONT":	// <SUBMITTER_TEXT>
						ValidateTokenData.IsValidString(line, 0, 248);
						var separator = line.Token.Equals("CONC") ? " " : "\r\n";

						if (!string.IsNullOrEmpty(text) && !text.EndsWith(separator))
							text += separator;

						text += line.Data;
                        line = GedcomLine.Read(stream);
                        break;

                    default:
                        line = ReadToken.ReadUnsupportedToken(stream, line);
                        break;
                }
            }

            return text;
        }
    }
}