using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace GedcomReader
{
	public class GedcomMedia : GedcomItem
    {
		public string Title { get; set; }
		public string Filename { get; set; }
		public string Format { get; set; }

		public override string ToString()
		{
			var toString = string.Format("{0}: {1}", ID, Title);
			return toString;
		}
	}
}
