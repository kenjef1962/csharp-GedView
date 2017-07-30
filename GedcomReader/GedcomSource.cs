using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace GedcomReader
{
    public class GedcomSource : GedcomItem
    {
		public string Authority { get; set; }
		public string Title { get; set; }
		public string Abbreviation { get; set; }
		public string Publication { get; set; }
		public string Text { get; set; }
		public string RepositoryID { get; set; }

		public override string ToString()
		{
			var toString = string.Format("{0}: {1}", ID, Title);
			return toString;
		}
	}
}
