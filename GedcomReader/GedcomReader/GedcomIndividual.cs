using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomIndividual : GedcomFactEntity
    {
        public List<GedcomName> Names { get; set; }
        public string Sex { get; set; }
        public List<string> FamilySpouseIDs { get; set; }
        public List<string> FamilyChildIDs { get; set; }
		public List<string> SubmitterIDs { get; set; }
		public string RFN { get; set; }
		public string AFN { get; set; }
		public string Photo { get; set; }

        public GedcomIndividual()
        {
            Names = new List<GedcomName>();
			FamilyChildIDs = new List<string>();
			FamilySpouseIDs = new List<string>();
			SubmitterIDs = new List<string>();
		}

		public bool IsValid()
		{
			if (string.IsNullOrEmpty(ID))
				return false;

			return true;
		}

        public static GedcomIndividual Read(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
            line = GedcomLine.Read(stream);

            if (!ReadToken.CanReadToken(stream, lineIn, "INDI"))
                return null;

            var gedcomIndividual = new GedcomIndividual();
            gedcomIndividual.ID = lineIn.Data;

            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case "NAME":	// <PERSONAL_NAME_STRUCTURE>
                        var name = GedcomName.Read(stream, line, out line);
                        if (name != null)
                        {
                            gedcomIndividual.Names.Add(name);
                        }
                        break;

                    case "SEX":		// <SEX_VALUE>
						ValidateTokenData.IsDuplicate(line, gedcomIndividual.Sex);
						ValidateTokenData.IsValidString(line, 1, 7);
						ValidateTokenData.LimitedValues(line, ValidateTokenData.ValidSexValues);
                        gedcomIndividual.Sex = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "FAMC":	// <CHILD_TO_FAMILY_LINK>
						ValidateTokenData.IsDuplicate(line, gedcomIndividual.FamilyChildIDs);
						ValidateTokenData.IsValidXref(line);
						gedcomIndividual.FamilyChildIDs.Add(line.Data);
						line = ReadToken_FAMC(stream, line, gedcomIndividual);
						break;

					case "FAMS":	// <SPOUSE_TO_FAMILY_LINK>
						ValidateTokenData.IsDuplicate(line, gedcomIndividual.FamilySpouseIDs);
						ValidateTokenData.IsValidXref(line);
						gedcomIndividual.FamilySpouseIDs.Add(line.Data);
						line = ReadToken_FAMS(stream, line, gedcomIndividual);
						break;

					case "SUBM":	// <SPOUSE_TO_FAMILY_LINK>
						ValidateTokenData.IsDuplicate(line, gedcomIndividual.SubmitterIDs);
						ValidateTokenData.IsValidXref(line);
						gedcomIndividual.SubmitterIDs.Add(line.Data);
						line = GedcomLine.Read(stream);
						break;

                    case "SOUR":	// <SOURCE_CITATION>
                        line = ReadToken.ReadCitation(stream, line, gedcomIndividual);
                        break;

                    case "OBJE":	// <MULTIMEDIA_LINK>
                        line = ReadToken.ReadMedia(stream, line, gedcomIndividual);
                        break;

					case "NOTE":	// <NOTE_STRUCTURE>
						line = ReadToken.ReadNote(stream, line, gedcomIndividual);
						break;

					case "RFN":		// <RECORD_FILE_NUMBER>
						ValidateTokenData.IsDuplicate(line, gedcomIndividual.RFN);
						ValidateTokenData.IsValidString(line, 1, 90);
						gedcomIndividual.RFN = line.Data;
						line = GedcomLine.Read(stream);
						break;

					case "AFN":		// <ANCESTRAL_FILE_NUMBER>
						ValidateTokenData.IsDuplicate(line, gedcomIndividual.AFN);
						ValidateTokenData.IsValidString(line, 1, 12);
						gedcomIndividual.AFN = line.Data;
						line = GedcomLine.Read(stream);
						break;

					case "REFN":	// <USER_REFERENCE_NUMBER>
					case "_UID":	// Gedcom Extension: User ID
						line = ReadToken.ReadReference(stream, line, gedcomIndividual);
						break;

					case "RIN":		// <AUTOMATED_RECORD_ID>
						line = ReadToken.ReadRIN(stream, line, gedcomIndividual);
						break;

					case "CHAN":	// <CHANGE_DATE>
						line = ReadToken.ReadChangeDate(stream, line, gedcomIndividual);
						break;

					case "_PHOTO":	// Gedcom Extension: Profile Photo
						ValidateTokenData.IsDuplicate(line, gedcomIndividual.Photo);
						ValidateTokenData.IsValidXref(line);
						gedcomIndividual.Photo = line.Data;
						line = GedcomLine.Read(stream);
						break;

                    default:
                        if (ValidateTokenData.ValidIndividualFacts.Contains(line.Token) ||
							ValidateTokenData.ValidIndividualFactsLDS.Contains(line.Token) ||
							ValidateTokenData.ValidCustomFacts.Contains(line.Token))
                        {
                            line = ReadToken.ReadFact(stream, line, gedcomIndividual);
                        }
                        else
                        {
                            line = ReadToken.ReadUnsupportedToken(stream, line);
                        }
                        break;
                }
            }

            return gedcomIndividual;
        }

		private static GedcomLine ReadToken_FAMC(StreamReader stream, GedcomLine lineIn, GedcomIndividual gedcomIndividual)
		{
			var line = GedcomLine.Read(stream);

			if (!ReadToken.CanReadToken(stream, lineIn, "FAMC"))
				return line;

			while ((line != null) && (lineIn.Level < line.Level))
			{
				switch (line.Token)
				{
					default:
						line = ReadToken.ReadUnsupportedToken(stream, line);
						break;
				}
			}

			return line;
		}

		private static GedcomLine ReadToken_FAMS(StreamReader stream, GedcomLine lineIn, GedcomIndividual gedcomIndividual)
		{
			var line = GedcomLine.Read(stream);

			if (!ReadToken.CanReadToken(stream, lineIn, "FAMS"))
				return line;

			while ((line != null) && (lineIn.Level < line.Level))
			{
				switch (line.Token)
				{
					default:
						line = ReadToken.ReadUnsupportedToken(stream, line);
						break;
				}
			}

			return line;
		}
	}
}
