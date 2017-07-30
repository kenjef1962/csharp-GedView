using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomAddress : GedcomEntity
    {
        public string Address { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

		public bool IsValid()
		{
			return true;
		}

		public static GedcomAddress Read(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
			line = GedcomLine.Read(stream);

			if (!ReadToken.CanReadToken(stream, lineIn, "ADDR"))
				return null;

            var gedcomAddress = new GedcomAddress();

			// <ADDRESS_LINE>
			ValidateTokenData.IsValidString(lineIn, 1, 60);
			gedcomAddress.Address = lineIn.Data;

            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case "CONT":	// <ADDRESS_LINE>
						ValidateTokenData.IsValidString(line, 1, 60);
						gedcomAddress.Address += "\r\n" + line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "ADR1":	// <ADDRESS_LINE1>
						ValidateTokenData.IsDuplicate(line, gedcomAddress.Address1);
						ValidateTokenData.IsValidString(line, 1, 60);
						gedcomAddress.Address1 = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "ADR2":	// <ADDRESS_LINE2>
						ValidateTokenData.IsDuplicate(line, gedcomAddress.Address2);
						ValidateTokenData.IsValidString(line, 1, 60);
						gedcomAddress.Address2 = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "CITY":	// <ADDRESS_CITY>
						ValidateTokenData.IsDuplicate(line, gedcomAddress.City);
						ValidateTokenData.IsValidString(line, 1, 60);
						gedcomAddress.City = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "STAT":	// <ADDRESS_STATE>
						ValidateTokenData.IsDuplicate(line, gedcomAddress.State);
						ValidateTokenData.IsValidString(line, 1, 60);
						gedcomAddress.State = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "POST":	// <ADDRESS_POSTAL_CODE>
						ValidateTokenData.IsDuplicate(line, gedcomAddress.PostalCode);
						ValidateTokenData.IsValidString(line, 1, 10);
						gedcomAddress.PostalCode = line.Data;
                        line = GedcomLine.Read(stream);
                        break;

					case "CTRY":	// <ADDRESS_COUNTRY>
						ValidateTokenData.IsDuplicate(line, gedcomAddress.Country);
						ValidateTokenData.IsValidString(line, 1, 60);
						gedcomAddress.Country = line.Data;
						line = GedcomLine.Read(stream);
						break;

                    default:
                        line = ReadToken.ReadUnsupportedToken(stream, line);
                        break;
                }
            }

            return gedcomAddress;
        }
    }
}
