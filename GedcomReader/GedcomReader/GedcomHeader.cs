using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomHeader
    {
        public string Source { get; set; }
        public string SourceVersion { get; set; }
        public string SourceProduct { get; set; }
		public string SourceCompany { get; set; }
		public GedcomAddress SourceAddress { get; set; }
		public string Desintation { get; set; }
        public string TransmissionDate { get; set; }
		public string TransmissionTime { get; set; }
		public string SubmitterID { get; set; }
        public string SubmissionID { get; set; }
        public string Filename { get; set; }
        public string Copyright { get; set; }
        public string GedcomVersion { get; set; }
        public string GedcomForm { get; set; }
		public string Charset { get; set; }
		public string CharsetVersion { get; set; }
		public string Language { get; set; }
        public string Note { get; set; }

		public GedcomHeader()
		{
		}

		public bool IsValid()
		{
			if (string.IsNullOrEmpty(Source))
				return false;

			if (string.IsNullOrEmpty(SubmitterID))
				return false;

			if (string.IsNullOrEmpty(GedcomVersion))
				return false;

			if (string.IsNullOrEmpty(GedcomForm))
				return false;

			if (string.IsNullOrEmpty(Charset))
				return false;

			return true;
		}

        public static GedcomHeader Read(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
			line = GedcomLine.Read(stream);

			if (!ReadToken.CanReadToken(stream, lineIn, "HEAD"))
				return null;

            var gedcomHeader = new GedcomHeader();

            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case "SOUR":	// <APPROVED_SYSTEM_ID>, required
						ValidateTokenData.IsDuplicate(line, gedcomHeader.Source);
						ValidateTokenData.IsValidString(line, 1, 20);
						gedcomHeader.Source = line.Data;
                        line = GedcomHeader.ReadToken_SOUR(stream, line, gedcomHeader);
                        break;

                    case "DEST":	// <RECEIVING_SYSTEM_NAME>
						ValidateTokenData.IsDuplicate(line, gedcomHeader.Desintation);
						ValidateTokenData.IsValidString(line, 1, 20);
                        gedcomHeader.Desintation = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

                    case "DATE":	// <TRANSMISSION_DATE>
						ValidateTokenData.IsDuplicate(line, gedcomHeader.TransmissionDate);
						ValidateTokenData.IsValidString(line, 1, 40);
						gedcomHeader.TransmissionDate = line.Data;
						line = GedcomHeader.ReadToken_DATE(stream, line, gedcomHeader);
                        break;

                    case "SUBM":	// <XREF:SUBM>, required
						ValidateTokenData.IsDuplicate(line, gedcomHeader.SubmitterID);
						ValidateTokenData.IsValidXref(line);
                        gedcomHeader.SubmitterID = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

                    case "SUBN":	// <XREF:SUBN>
						ValidateTokenData.IsDuplicate(line, gedcomHeader.SubmissionID);
						ValidateTokenData.IsValidXref(line);
                        gedcomHeader.SubmissionID = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

                    case "FILE":	// <FILE_NAME>
						ValidateTokenData.IsDuplicate(line, gedcomHeader.Filename);
						ValidateTokenData.IsValidString(line, 1, 90);
                        gedcomHeader.Filename = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

                    case "COPR":	// <COPYRIGHT_GEDCOM_FILE>
						ValidateTokenData.IsDuplicate(line, gedcomHeader.Copyright);
						ValidateTokenData.IsValidString(line, 1, 90);
                        gedcomHeader.Copyright = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

                    case "GEDC":	// Gedcom information, required
						line = GedcomHeader.ReadToken_GEDC(stream, line, gedcomHeader);
                        break;

                    case "CHAR":	// <CHARACTER_SET>, required
						ValidateTokenData.IsDuplicate(line, gedcomHeader.Charset);
						ValidateTokenData.IsValidString(line, 1, 8);
						gedcomHeader.Charset = line.Data;
						line = GedcomHeader.ReadToken_CHAR(stream, line, gedcomHeader);
                        break;

                    case "LANG":	// <LANGUAGE_OF_TEXT>
						ValidateTokenData.IsDuplicate(line, gedcomHeader.Language);
						ValidateTokenData.IsValidString(line, 1, 15);
                        gedcomHeader.Language = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

                    case "NOTE":	// <NOTE_STRUCTURE>
                        gedcomHeader.Note = GedcomText.Read(stream, line, out line);
                        break;

                    default:
                        line = ReadToken.ReadUnsupportedToken(stream, line);
                        break;
                }
            }

            return gedcomHeader;
        }

        public static GedcomLine ReadToken_SOUR(StreamReader stream, GedcomLine lineIn, GedcomHeader gedcomHeader)
        {
			var line = GedcomLine.Read(stream);

			if (!ReadToken.CanReadToken(stream, lineIn, "SOUR"))
				return line;

            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case "VERS":	// <VERSION_NUMBER>
						ValidateTokenData.IsDuplicate(line, gedcomHeader.SourceVersion);
						ValidateTokenData.IsValidString(line, 1, 15);
						gedcomHeader.SourceVersion = line.Data;
						line = GedcomLine.Read(stream);
                        break;

                    case "NAME":	// <NAME_OF_PRODUCT>
						ValidateTokenData.IsDuplicate(line, gedcomHeader.SourceProduct);
						ValidateTokenData.IsValidString(line, 1, 90);
						gedcomHeader.SourceProduct = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

                    case "CORP":	// <NAME_OF_BUSINESS>
						ValidateTokenData.IsDuplicate(line, gedcomHeader.SourceCompany);
						ValidateTokenData.IsValidString(line, 1, 90);
						gedcomHeader.SourceCompany = line.Data;
                        line = GedcomHeader.ReadToken_SOUR_CORP(stream, line, gedcomHeader);
                        break;

                    default:
                        line = ReadToken.ReadUnsupportedToken(stream, line);
                        break;
                }
            }

            return line;
        }

		public static GedcomLine ReadToken_SOUR_CORP(StreamReader stream, GedcomLine lineIn, GedcomHeader gedcomHeader)
        {
			var line = GedcomLine.Read(stream);

			if (!ReadToken.CanReadToken(stream, lineIn, "CORP")) 
				return line;

            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
					case "ADDR":	// <ADDRESS_STRUCTURE>
						ValidateTokenData.IsDuplicate(line, gedcomHeader.SourceAddress);
						gedcomHeader.SourceAddress = GedcomAddress.Read(stream, line, out line);
						break;

                    default:
                        line = ReadToken.ReadUnsupportedToken(stream, line);
                        break;
                }
            }

            return line;
        }

		public static GedcomLine ReadToken_DATE(StreamReader stream, GedcomLine lineIn, GedcomHeader gedcomHeader)
		{
			var line = GedcomLine.Read(stream);

			if (!ReadToken.CanReadToken(stream, lineIn, "DATE")) 
				return line;

			while ((line != null) && (lineIn.Level < line.Level))
			{
				switch (line.Token)
				{
					case "TIME":	// <TIME_VALUE>
						ValidateTokenData.IsDuplicate(line, gedcomHeader.TransmissionTime);
						ValidateTokenData.IsValidString(line, 1, 40);
						gedcomHeader.TransmissionTime = line.Data;
						line = GedcomLine.Read(stream);
						break;

					default:
						line = ReadToken.ReadUnsupportedToken(stream, line);
						break;
				}
			}

			return line;
		}

		public static GedcomLine ReadToken_GEDC(StreamReader stream, GedcomLine lineIn, GedcomHeader gedcomHeader)
		{
			var line = GedcomLine.Read(stream);

			if (!ReadToken.CanReadToken(stream, lineIn, "GEDC"))
				return line;

			while ((line != null) && (lineIn.Level < line.Level))
			{
				switch (line.Token)
				{
					case "VERS":	// <VERSION_NUMBER>
						ValidateTokenData.IsDuplicate(line, gedcomHeader.GedcomVersion);
						ValidateTokenData.IsValidString(line, 1, 15);
						gedcomHeader.GedcomVersion = line.Data;
						line = GedcomLine.Read(stream);
						break;

					case "FORM":	// <GEDCOM_FORM>
						ValidateTokenData.IsDuplicate(line, gedcomHeader.GedcomForm);
						ValidateTokenData.IsValidString(line, 1, 20);
						gedcomHeader.GedcomForm = line.Data;
						line = GedcomLine.Read(stream);
						break;

					default:
						line = ReadToken.ReadUnsupportedToken(stream, line);
						break;
				}
			}

			return line;
		}

		public static GedcomLine ReadToken_CHAR(StreamReader stream, GedcomLine lineIn, GedcomHeader gedcomHeader)
		{
			var line = GedcomLine.Read(stream);

			if (!ReadToken.CanReadToken(stream, lineIn, "CHAR"))
				return line;

            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case "VERS":	// <VERSION_NUMBER>
						ValidateTokenData.IsDuplicate(line, gedcomHeader.CharsetVersion);
						ValidateTokenData.IsValidString(line, 1, 15);
						gedcomHeader.CharsetVersion = line.Data;
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
