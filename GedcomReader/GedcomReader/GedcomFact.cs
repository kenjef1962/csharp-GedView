using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomFact : GedcomEntity
    {
        public string Type { get; set; }
        public string Date { get; set; }
        public string Place { get; set; }
		public GedcomAddress Address { get; set; }
		public string Temple { get; set; }
		public string Status { get; set; }
		public string AgeAtEvent { get; set; }
		public string CauseOfEvent { get; set; }
		public bool IsCustom { get; set; }
        public bool IsShared { get; set; }
        public bool IsLDS { get; set; }

        public string PrimaryID { get; set; }
        public string SecondaryID { get; set; }

        internal static int CurrentID = 1;

		public bool IsValid()
		{
			if (string.IsNullOrEmpty(ID))
				return false;

			if (string.IsNullOrEmpty(Type))
				return false;

			return true;
		}

        public static GedcomFact Read(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
            line = GedcomLine.Read(stream);

            if ((stream == null) || (lineIn == null))
            { 
                return null; 
            }

            // Check for valid facts
			if (!ValidateTokenData.ValidIndividualFacts.Contains(lineIn.Token) &&
				!ValidateTokenData.ValidIndividualFactsLDS.Contains(lineIn.Token) &&
				!ValidateTokenData.ValidFamilyFacts.Contains(lineIn.Token) &&
				!ValidateTokenData.ValidFamilyFactsLDS.Contains(lineIn.Token) &&
				!ValidateTokenData.ValidCustomFacts.Contains(lineIn.Token))
            {
                return null;
            }

            var gedcomFact = new GedcomFact();
            gedcomFact.ID = string.Format("@E{0}@", CurrentID++);
            gedcomFact.Type = lineIn.Token;

			gedcomFact.IsCustom = ValidateTokenData.ValidCustomFacts.Contains(lineIn.Token);
			gedcomFact.IsShared = (ValidateTokenData.ValidFamilyFacts.Contains(lineIn.Token) || ValidateTokenData.ValidFamilyFactsLDS.Contains(lineIn.Token));
			gedcomFact.IsLDS = (ValidateTokenData.ValidIndividualFactsLDS.Contains(lineIn.Token) || ValidateTokenData.ValidFamilyFactsLDS.Contains(lineIn.Token));

            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case "TYPE":	// <EVENT_DISCRIPTOR>
						ValidateTokenData.IsValidString(line, 1, 90);
                        gedcomFact.Type = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

                    case "DATE":	// <DATE_VALUE>
						ValidateTokenData.IsDuplicate(line, gedcomFact.Date);
						ValidateTokenData.IsValidString(line, 1, 40);
                        gedcomFact.Date = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

                    case "PLAC":	// <PLACE_STRUCTURE>
						ValidateTokenData.IsDuplicate(line, gedcomFact.Place);
						ValidateTokenData.IsValidString(line, 1, 90);
                        gedcomFact.Place = line.Data;
						line = GedcomFact.ReadToken_PLAC(stream, line, gedcomFact);
                        break;

					case "ADDR":	// <ADDRESS_STRUCTURE>
						ValidateTokenData.IsDuplicate(line, gedcomFact.Address);
						gedcomFact.Address = GedcomAddress.Read(stream, line, out line);
						break;

					case "TEMP":	// <TEMPLE_CODE>
						ValidateTokenData.IsDuplicate(line, gedcomFact.Temple);
						ValidateTokenData.IsValidString(line, 1, 5);
						gedcomFact.Temple = line.Data;
						line = GedcomLine.Read(stream);
						break;

					case "STAT":	// <LDS_DATE_STATUS>
						ValidateTokenData.IsDuplicate(line, gedcomFact.Status);
						ValidateTokenData.IsValidString(line, 3, 10);
						ValidateTokenData.LimitedValues(line, ValidateTokenData.ValidOrdinanceStatusLDS);
						gedcomFact.Status = line.Data;
						line = GedcomLine.Read(stream);
						break;

					case "AGE":		// <AGE_AT_EVENT>
						ValidateTokenData.IsDuplicate(line, gedcomFact.AgeAtEvent);
						ValidateTokenData.IsValidString(line, 1, 12);
						gedcomFact.AgeAtEvent = line.Data;
						line = GedcomLine.Read(stream);
						break;

					case "CAUS":	// <CAUSE_OF_EVENT>
						ValidateTokenData.IsDuplicate(line, gedcomFact.CauseOfEvent);
						ValidateTokenData.IsValidString(line, 1, 90);
						gedcomFact.CauseOfEvent = line.Data;
						line = GedcomLine.Read(stream);
						break;

                    case "SOUR":	// <SOURCE_CITATION>
                        line = ReadToken.ReadCitation(stream, line, gedcomFact);
                        break;

                    case "OBJE":	// <MULTIMEDIA_LINK>
                        line = ReadToken.ReadMedia(stream, line, gedcomFact);
                        break;

                    case "NOTE":	// <NOTE_STRUCTURE>
                        line = ReadToken.ReadNote(stream, line, gedcomFact);
                        break;

                    default:
                        line = ReadToken.ReadUnsupportedToken(stream, line);
                        break;
                }
            }

            return gedcomFact;
        }

		private static GedcomLine ReadToken_PLAC(StreamReader stream, GedcomLine lineIn, GedcomFact gedcomFact)
		{
			var line = GedcomLine.Read(stream);

			if (!ReadToken.CanReadToken(stream, lineIn, "PLAC"))
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
