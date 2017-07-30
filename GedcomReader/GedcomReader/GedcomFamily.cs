using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomFamily : GedcomFactEntity
    {
        public string HusbandID { get; set; }
        public string WifeID { get; set; }
		public List<string> ChildIDs { get; set; }
		public string NoChildren { get; set; }
		public List<string> SubmitterIDs { get; set; }

        public GedcomFamily()
        {
			ChildIDs = new List<string>();
			SubmitterIDs = new List<string>();
		}

		public bool IsValid()
		{
			if (string.IsNullOrEmpty(ID))
				return false;

			return true;
		}

        public static GedcomFamily Read(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
            line = GedcomLine.Read(stream);

            if (!ReadToken.CanReadToken(stream, lineIn, "FAM"))
                return null;

            var gedcomFamily = new GedcomFamily();
            gedcomFamily.ID = lineIn.Data;

            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case "HUSB":	// <XREF:INDI>
						ValidateTokenData.IsDuplicate(line, gedcomFamily.HusbandID);
						ValidateTokenData.IsValidXref(line);
                        gedcomFamily.HusbandID = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "WIFE":	// <XREF:INDI>
						ValidateTokenData.IsDuplicate(line, gedcomFamily.WifeID);
						ValidateTokenData.IsValidXref(line);
                        gedcomFamily.WifeID = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "CHIL":	// <XREF:INDI>
						ValidateTokenData.IsDuplicate(line, gedcomFamily.ChildIDs);
						ValidateTokenData.IsValidXref(line);
                        gedcomFamily.ChildIDs.Add(line.Data);
						line = ReadToken_CHIL(stream, line, gedcomFamily);
                        break;

					case "NCHI":	// <COUNT_OF_CHILDREN>
						ValidateTokenData.IsDuplicate(line, gedcomFamily.NoChildren);
						ValidateTokenData.IsValidString(line, 1, 3);
						gedcomFamily.NoChildren = line.Data;
						line = GedcomLine.Read(stream);
						break;

					case "SUBM":	// <XREF:SUBM>
						ValidateTokenData.IsDuplicate(line, gedcomFamily.SubmitterIDs);
						ValidateTokenData.IsValidXref(line);
						gedcomFamily.SubmitterIDs.Add(line.Data);
						line = GedcomLine.Read(stream);
						break;

                    case "SOUR":	// <SOURCE_CITATION>
						line = ReadToken.ReadCitation(stream, line, gedcomFamily);
                        break;

                    case "OBJE":	// <MULTIMEDIA_LINK>
						line = ReadToken.ReadMedia(stream, line, gedcomFamily);
                        break;

                    case "NOTE":	// <NOTE_STRUCTURE>
						line = ReadToken.ReadNote(stream, line, gedcomFamily);
                        break;

					case "REFN":	// <USER_REFERENCE_NUMBER>
					case "_UID":	// Gedcom Extension: User ID
						line = ReadToken.ReadReference(stream, line, gedcomFamily);
						break;

					case "RIN":		// <AUTOMATED_RECORD_ID>
						line = ReadToken.ReadRIN(stream, line, gedcomFamily);
						break;

					case "CHAN":	// <CHANGE_DATE>
						line = ReadToken.ReadChangeDate(stream, line, gedcomFamily);
						break;

                    default:		// <FAMILY_EVENT_STRUCTURE>, <LDS_SPOUSE_SEALING>
						if (ValidateTokenData.ValidFamilyFacts.Contains(line.Token) ||
							ValidateTokenData.ValidFamilyFactsLDS.Contains(line.Token) ||
							ValidateTokenData.ValidCustomFacts.Contains(line.Token))
						{
							line = ReadToken.ReadFact(stream, line, gedcomFamily);
                        }
                        else
                        {
                            line = ReadToken.ReadUnsupportedToken(stream, line);
                        }
                        break;
                }
            }

            return gedcomFamily;
        }

		public static GedcomLine ReadToken_CHIL(StreamReader stream, GedcomLine lineIn, GedcomFamily gedcomFamily)
		{
			var line = GedcomLine.Read(stream);

			if (!ReadToken.CanReadToken(stream, lineIn, "CHIL"))
				return line;

			while ((line != null) && (lineIn.Level < line.Level))
			{
				switch (line.Token)
				{
					//case "_FREL":	// Gedcom Extension: Father Relationship
					//case "_MREL":	// Gedcom Extension: Mother Relationship
					//	line = GedcomLine.Read(stream);
					//	break;

					default:
						line = ReadToken.ReadUnsupportedToken(stream, line);
						break;
				}
			}

			return line;
		}
	}
}
