using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomRepository : GedcomEntity
    {
        public string Name { get; set; }
        public GedcomAddress Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

		public bool IsValid()
		{
			return true;
		}

        public static GedcomRepository Read(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
            line = GedcomLine.Read(stream);

            if (!ReadToken.CanReadToken(stream, lineIn, "REPO"))
                return null;

            var gedcomRepository = new GedcomRepository();
            gedcomRepository.ID = lineIn.Data;

            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case "NAME":	// <NAME_OF_REPOSITORY>
						ValidateTokenData.IsDuplicate(line, gedcomRepository.Name);
						ValidateTokenData.IsValidString(line, 1, 90);
                        gedcomRepository.Name = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

                    case "ADDR":	// <ADDRESS_STRUCTURE>
						ValidateTokenData.IsDuplicate(line, gedcomRepository.Address);
                        gedcomRepository.Address = GedcomAddress.Read(stream, line, out line);
                        break;

                    case "PHON":	// <PHONE_NUMBER>
						ValidateTokenData.IsDuplicate(line, gedcomRepository.Phone);
						ValidateTokenData.IsValidString(line, 1, 40);
                        gedcomRepository.Phone = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "EMAIL":	// Gedcom Extension: Email
						ValidateTokenData.IsDuplicate(line, gedcomRepository.Email);
						ValidateTokenData.IsValidString(line, 1, 90);
                        gedcomRepository.Email = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "NOTE":	// <NOTE_STRUCTURE>
						line = ReadToken.ReadNote(stream, line, gedcomRepository);
						break;

					case "REFN":	// <USER_REFERENCE_NUMBER>
						line = ReadToken.ReadReference(stream, line, gedcomRepository);
						break;

					case "RIN":		// <AUTOMATED_RECORD_ID>
						line = ReadToken.ReadRIN(stream, line, gedcomRepository);
						break;

					case "CHAN":	// <CHANGE_DATE>
						line = ReadToken.ReadChangeDate(stream, line, gedcomRepository);
						break;

                    default:
                        line = ReadToken.ReadUnsupportedToken(stream, line);
                        break;
                }
            }

            return gedcomRepository;
        }
    }
}
