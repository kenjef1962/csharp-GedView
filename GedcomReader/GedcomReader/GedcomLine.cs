using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomLine
    {
        public int LineNo { get; set; }
        public int Level { get; set; }
        public string Token { get; set; }
        public string Data { get; set; }
        public bool IsXref { get; set; }

        internal static int CurrentLineNo = 1;

		public bool IsValid()
		{
			if (Level < 0)
				return false;

			if (string.IsNullOrEmpty(Token))
				return false;

			return true;
		}
		
        public static GedcomLine Read(StreamReader stream)
        {
            if (stream == null)
                return null;

            var line = stream.ReadLine();

            if (string.IsNullOrEmpty(line)) 
                return null;

            var lineParts = line.Trim().Split(new[] { ' ' }, 3);

            var level = 0;
            if (!int.TryParse(lineParts[0], out level))
                return null;

            var gedcomLine = new GedcomLine();
            gedcomLine.LineNo = CurrentLineNo++;
            gedcomLine.Level = level;
            gedcomLine.Token = (1 < lineParts.Length) ? lineParts[1].ToUpper() : string.Empty;
            gedcomLine.Data = (2 < lineParts.Length) ? lineParts[2] : string.Empty;
            gedcomLine.IsXref = false;

            if (gedcomLine.Token.StartsWith("@"))
            {
                var temp = gedcomLine.Token;
                gedcomLine.Token = gedcomLine.Data;
                gedcomLine.Data = temp;
                gedcomLine.IsXref = true;
            }

            return gedcomLine;
        }
    }
}
