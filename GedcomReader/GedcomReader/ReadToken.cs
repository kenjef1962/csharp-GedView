using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class ReadToken
    {
        public static bool CanReadToken(StreamReader stream, GedcomLine lineIn, string token)
        {
            if ((stream == null) || (lineIn == null))
                return false;

            if (lineIn.Token != token)
                return false;

            return true;
        }

        public static GedcomLine ReadFact(StreamReader stream, GedcomLine line, GedcomFactEntity gedcomEntity)
        {
            var data = GedcomFact.Read(stream, line, out line);
            if (data != null)
            {
                if (gedcomEntity is GedcomIndividual)
                {
                    var gedcomIndividual = gedcomEntity as GedcomIndividual;
                    data.PrimaryID = gedcomEntity.ID;
                    data.IsShared = false;
                }
                else if (gedcomEntity is GedcomFamily)
                {
                    var gedcomFamily = gedcomEntity as GedcomFamily;
                    data.PrimaryID = gedcomFamily.HusbandID;
                    data.SecondaryID = gedcomFamily.WifeID;
                    data.IsShared = true;
                }

                gedcomEntity.Facts.Add(data);
            }

            return line;
        }

        public static GedcomLine ReadCitation(StreamReader stream, GedcomLine line, GedcomEntity gedcomEntity)
        {
            var data = GedcomCitation.Read(stream, line, out line);
            if (data != null)
            {
                gedcomEntity.Citations.Add(data);
            }

            return line;
        }

        public static GedcomLine ReadMedia(StreamReader stream, GedcomLine line, GedcomEntity gedcomEntity)
        {
            var data = GedcomMedia.Read(stream, line, out line);
            if (data != null)
            {
                gedcomEntity.Media.Add(data);
            }

            return line;
        }

        public static GedcomLine ReadNote(StreamReader stream, GedcomLine line, GedcomEntity gedcomEntity)
        {
            var data = GedcomNote.Read(stream, line, out line);
            if (data != null)
            {
                gedcomEntity.Notes.Add(data);
            }

            return line;
        }

		public static GedcomLine ReadReference(StreamReader stream, GedcomLine line, GedcomEntity gedcomEntity)
        {
			if (!string.IsNullOrEmpty(line.Data))
			{
				ValidateTokenData.IsDuplicate(line, gedcomEntity.ReferenceNumbers);
				ValidateTokenData.IsValidString(line, 1, 40);
				gedcomEntity.ReferenceNumbers.Add(line.Data);
			}

			return GedcomLine.Read(stream);
        }

		public static GedcomLine ReadRIN(StreamReader stream, GedcomLine line, GedcomEntity gedcomEntity)
		{
			if (!string.IsNullOrEmpty(line.Data))
			{
				ValidateTokenData.IsDuplicate(line, gedcomEntity.RIN);
				ValidateTokenData.IsValidString(line, 1, 12);
				gedcomEntity.RIN = line.Data;
			}

			return GedcomLine.Read(stream);
		}

		public static GedcomLine ReadChangeDate(StreamReader stream, GedcomLine line, GedcomEntity gedcomEntity)
		{
			var data = GedcomChange.Read(stream, line, out line);
			if (data != null)
			{
				ValidateTokenData.IsDuplicate(line, gedcomEntity.ChangeDate);
				gedcomEntity.ChangeDate = data;
			}

			return line;
		}

        public static GedcomLine ReadUnsupportedToken(StreamReader stream, GedcomLine lineIn, string message = "Unsupported token")
        {
			if (lineIn.Token.StartsWith("_"))
				message += " (custom)";

            var logItem = new GedcomLogItem(message, lineIn);

            var line = GedcomLine.Read(stream);
            while ((line != null) && (lineIn.Level < line.Level))
            {
                logItem.Lines.Add(line);
                line = GedcomLine.Read(stream);
            }

            Gedcom.WriteLogItem(logItem);

            return line;
        }
    }
}
