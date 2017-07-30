using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomName : GedcomEntity
    {
        public string Fullname { get; set; }
        public string Prefix { get; set; }
        public string Given { get; set; }
        public string Nickname { get; set; }
        public string SurnamePrefix { get; set; }
        public string Surname { get; set; }
        public string Suffix { get; set; }

		public bool IsValid()
		{
			if (string.IsNullOrEmpty(Fullname))
				return false;

			return true;
		}

        public static GedcomName Read(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
            line = GedcomLine.Read(stream);

            if (!ReadToken.CanReadToken(stream, lineIn, "NAME"))
                return null;

            var gedcomName = new GedcomName();

			ValidateTokenData.IsDuplicate(lineIn, gedcomName.Fullname);
			ValidateTokenData.IsValidString(lineIn, 1, 120);
            gedcomName.Fullname = lineIn.Data;

            ParseFullname(gedcomName);

            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case "NPFX":	// <NAME_PIECE_PREFIX>
						ValidateTokenData.IsDuplicate(lineIn, gedcomName.Prefix);
						ValidateTokenData.IsValidString(lineIn, 1, 30);
                        gedcomName.Prefix = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "GIVN":	// <NAME_PIECE_GIVEN>
						//ValidateTokenData.IsDuplicate(lineIn, gedcomName.Given);
						ValidateTokenData.IsValidString(lineIn, 1, 120);
                        gedcomName.Given = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "NICK":	// <NAME_PIECE_NICKNAME>
						ValidateTokenData.IsDuplicate(lineIn, gedcomName.Nickname);
						ValidateTokenData.IsValidString(lineIn, 1, 30);
                        gedcomName.Nickname = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "SPFX":	// <NAME_PIECE_SURNAME_PREFIX>
						ValidateTokenData.IsDuplicate(lineIn, gedcomName.SurnamePrefix);
						ValidateTokenData.IsValidString(lineIn, 1, 30);
                        gedcomName.SurnamePrefix = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "SURN":	// <NAME_PIECE_SURNAME>
						//ValidateTokenData.IsDuplicate(lineIn, gedcomName.Surname);
						ValidateTokenData.IsValidString(lineIn, 1, 120);
                        gedcomName.Surname = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "NSFX":	// <NAME_PIECE_SUFIX>
						ValidateTokenData.IsDuplicate(lineIn, gedcomName.Suffix);
						ValidateTokenData.IsValidString(lineIn, 1, 30);
                        gedcomName.Suffix = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

                    case "SOUR":	// <SOURCE_CITATION>
                        line = ReadToken.ReadCitation(stream, line, gedcomName);
                        break;

                    case "NOTE":	// <NOTE_STRUCTURE>
                        line = ReadToken.ReadNote(stream, line, gedcomName);
                        break;

                    default:
                        line = ReadToken.ReadUnsupportedToken(stream, line);
                        break;
                }
            }

            return gedcomName;
        }

        private static void ParseFullname(GedcomName gedcomName)
        {
            if ((gedcomName == null) || string.IsNullOrEmpty(gedcomName.Fullname))
                return;

            var fullname = gedcomName.Fullname.Trim();

			if (fullname.Contains("/"))
			{
				var idx1 = fullname.IndexOf("/");
				var idx2 = fullname.LastIndexOf("/");

				var surname = fullname.Substring(idx1, idx2 - idx1 + 1);
				gedcomName.Surname = surname.Replace("/", string.Empty).Trim();
				
				var given = gedcomName.Fullname.Replace(surname, string.Empty);
				gedcomName.Given = given.Replace("  ", " ").Trim();
			}
			else
			{
				var nameParts = fullname.Split(new[] { ' ' });

				var surname = nameParts[nameParts.Length - 1];
				gedcomName.Surname = surname.Replace("/", string.Empty).Trim();
				gedcomName.Given = gedcomName.Fullname.Replace(surname, string.Empty).Trim();
			}
        }
    }
}
