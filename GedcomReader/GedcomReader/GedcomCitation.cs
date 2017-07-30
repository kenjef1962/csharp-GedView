using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomCitation : GedcomEntity
    {
        public string SourceID { get; set; }
        public string Page { get; set; }
        public string Data { get; set; }
        public string Quality { get; set; }
		public string Footnote { get; set; }

        internal static int CurrentID = 1;

		public bool IsValid()
		{
			return true;
		}

        public static GedcomCitation Read(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
            line = GedcomLine.Read(stream);

            if (!ReadToken.CanReadToken(stream, lineIn, "SOUR"))
				return null;

            var gedcomCitation = new GedcomCitation();
            gedcomCitation.ID = string.Format("@C{0}@", CurrentID++);

			ValidateTokenData.IsValidXref(lineIn);
            gedcomCitation.SourceID = lineIn.Data;

            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case "PAGE":	// <WHERE_WITHIN_SOURCE>
						ValidateTokenData.IsDuplicate(line, gedcomCitation.Page);
						ValidateTokenData.IsValidString(line, 1, 248);
                        gedcomCitation.Page = GedcomText.Read(stream, line, out line);
                        break;

					case "DATA":	// <TEXT_FROM_SOURCE>
					case "TEXT":	// <TEXT_FROM_SOURCE>
						ValidateTokenData.IsDuplicate(line, gedcomCitation.Data);
                        gedcomCitation.Data = GedcomText.Read(stream, line, out line);
                        break;

                    case "QUAY":	// <CERTAINTY_ASSESSMENT>
						ValidateTokenData.IsDuplicate(line, gedcomCitation.Quality);
						ValidateTokenData.IsValidString(line, 1, 25);
                        gedcomCitation.Quality = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

                    case "OBJE":	// <MULTIMEIDA_LINK>
                        line = ReadToken.ReadMedia(stream, line, gedcomCitation);
                        break;

                    case "NOTE":	// <NOTE_STRUCTURE>
                        line = ReadToken.ReadNote(stream, line, gedcomCitation);
                        break;

					case "_FOOT":	// Gedcom Extension: Footnote
						ValidateTokenData.IsDuplicate(line, gedcomCitation.Footnote);
						ValidateTokenData.IsValidString(line, 1, 248);
						gedcomCitation.Footnote = GedcomText.Read(stream, line, out line);
						break;

                    default:
                        line = ReadToken.ReadUnsupportedToken(stream, line);
                        break;
                }
            }

            return gedcomCitation;
        }
    }
}
