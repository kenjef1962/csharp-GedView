using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace GedcomReader
{
    public class GedcomLine
    {
        public int Level { get; set; }
        public string Token { get; set; }
        public string Data { get; set; }

        public bool IsXref { get; set; }

		public override string ToString()
		{
			var toString = string.Format("{0} {1} {2}", Level, Token, Data);
			return toString;
		}
    }
}
