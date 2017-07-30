using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomSource : GedcomEntity
    {
        public string Author { get; set; }
		public string Title { get; set; }
		public string Abbreviation { get; set; }
		public string Publisher { get; set; }
		public string Text { get; set; }
		public GedcomRepository Repository { get; set; }

        public static GedcomSource Read(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
            line = GedcomLine.Read(stream);

            if (!ReadToken.CanReadToken(stream, lineIn, "SOUR")) 
                return null;

            var gedcomSource = new GedcomSource();
            gedcomSource.ID = lineIn.Data;

            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case "AUTH":	// <SOURCE_ORIGINATOR>
						ValidateTokenData.IsDuplicate(line, gedcomSource.Author);
						ValidateTokenData.IsValidString(line, 1, 248);
                        gedcomSource.Author = GedcomText.Read(stream, line, out line);
                        break;

                    case "TITL":	// <SOURCE_DESCRIPTIVE_TITLE>
						ValidateTokenData.IsDuplicate(line, gedcomSource.Title);
						ValidateTokenData.IsValidString(line, 1, 248);
                        gedcomSource.Title = GedcomText.Read(stream, line, out line);
                        break;

					case "ABBR":	// <SOURCE_FILED_BY_ENTRY>
						ValidateTokenData.IsDuplicate(line, gedcomSource.Abbreviation);
						ValidateTokenData.IsValidString(line, 1, 60);
						gedcomSource.Abbreviation = line.Data;
						line = GedcomLine.Read(stream);
						break;

                    case "PUBL":	// <SOURCE_PUBLICATIONS_FACTS>
						ValidateTokenData.IsDuplicate(line, gedcomSource.Publisher);
						ValidateTokenData.IsValidString(line, 1, 248);
                        gedcomSource.Publisher = GedcomText.Read(stream, line, out line);
                        break;

                    case "TEXT":	// <TEXT_FROM_SOURCE>
						ValidateTokenData.IsDuplicate(line, gedcomSource.Text);
						ValidateTokenData.IsValidString(line, 1, 248);
                        gedcomSource.Text = GedcomText.Read(stream, line, out line);
                        break;

                    case "REPO":	// <REPOSITORY_RECORD>
						ValidateTokenData.IsDuplicate(line, gedcomSource.Repository);
                        gedcomSource.Repository = GedcomRepository.Read(stream, line, out line);
                        break;

                    case "OBJE":	// <MULTIMEDIA_LINK>
                        line = ReadToken.ReadMedia(stream, line, gedcomSource);
                        break;

                    case "NOTE":	// <NOTE_STRUCTURE>
                        line = ReadToken.ReadNote(stream, line, gedcomSource);
                        break;

					case "REFN":	// <USER_REFERENCE_NUMBER>
						line = ReadToken.ReadReference(stream, line, gedcomSource);
						break;

					case "RIN":		// <AUTOMATED_RECORD_ID>
						line = ReadToken.ReadRIN(stream, line, gedcomSource);
						break;

					case "CHAN":	// <CHANGE_DATE>
						line = ReadToken.ReadChangeDate(stream, line, gedcomSource);
						break;

                    default:
                        line = ReadToken.ReadUnsupportedToken(stream, line);
                        break;
                }
            }

            return gedcomSource;
        }
    }
}
