using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomNote : GedcomEntity
    {
        public string Text { get; set; }

		public bool IsValid()
		{
			if (string.IsNullOrEmpty(Text))
				return false;

			return true;
		}

        public static GedcomNote Read(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
            // Do not read a line ahead as this will happen in the GedcomText.Read(...)
            //line = GedcomLine.Read(stream);

            if (!ReadToken.CanReadToken(stream, lineIn, "NOTE"))
            {
                line = GedcomLine.Read(stream);
                return null; 
            }

            var gedcomNote = new GedcomNote();

			if (lineIn.IsXref)
            {
				ValidateTokenData.IsValidXref(lineIn);
				gedcomNote.ID = lineIn.Data;
	            lineIn.Data = string.Empty;
			}

            gedcomNote.Text = GedcomText.Read(stream, lineIn, out line);

            return gedcomNote;
        }
    }
}
