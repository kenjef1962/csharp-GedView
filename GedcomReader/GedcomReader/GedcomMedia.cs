using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomMedia : GedcomEntity
    {
        public string Format { get; set; }
        public string Title { get; set; }
        public string Filename { get; set; }
        public string Date { get; set; }
        public string Text { get; set; }

		public bool IsValid()
		{
			if (string.IsNullOrEmpty(Filename))
				return false;

			return true;
		}

        public static GedcomMedia Read(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
            line = GedcomLine.Read(stream);

            if (!ReadToken.CanReadToken(stream, lineIn, "OBJE"))
                return null;

            var gedcomMedia = new GedcomMedia();
            gedcomMedia.ID = lineIn.Data;

            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case "FORM":	// <MULTIMEDIA_FORMAT>
						ValidateTokenData.IsDuplicate(line, gedcomMedia.Format);
						ValidateTokenData.IsValidString(line, 1, 1);
                        gedcomMedia.Format = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

                    case "TITL":	// <DESCRIPTIVE_TEXT>
						ValidateTokenData.IsDuplicate(line, gedcomMedia.Format);
						ValidateTokenData.IsValidString(line, 1, 248);
                        gedcomMedia.Title = GedcomText.Read(stream, line, out line);
                        break;

                    case "FILE":	// <MULTIMEDIA_FILE_REFERENCE>
						ValidateTokenData.IsDuplicate(line, gedcomMedia.Format);
						ValidateTokenData.IsValidString(line, 1, 248);
                        gedcomMedia.Filename = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "DATE":	// <DATE_VALUE>
						ValidateTokenData.IsDuplicate(line, gedcomMedia.Format);
						ValidateTokenData.IsValidString(line, 1, 40);
                        gedcomMedia.Date = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

                    case "TEXT":	// <Text>
						ValidateTokenData.IsDuplicate(line, gedcomMedia.Text);
						ValidateTokenData.IsValidString(line, 1, 248);
                        gedcomMedia.Text = GedcomText.Read(stream, line, out line);
                        break;

					case "SOUR":	// <SOURCE_CITATION>
                        line = ReadToken.ReadCitation(stream, line, gedcomMedia);
						break;

                    case "NOTE":	// <NOTE_STRUCTURE>
                        line = ReadToken.ReadNote(stream, line, gedcomMedia);
                        break;

                    default:
                        line = ReadToken.ReadUnsupportedToken(stream, line);
                        break;
                }
            }

            return gedcomMedia;
        }
    }
}
