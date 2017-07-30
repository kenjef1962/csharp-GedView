using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace GedcomReader
{
    public class GedcomNote : GedcomItem
    {
		public string Text { get; set; }

		public override string ToString()
		{
			return Text;
		}
	}
}
