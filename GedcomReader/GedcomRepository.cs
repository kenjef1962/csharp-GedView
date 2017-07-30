using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace GedcomReader
{
    public class GedcomRepository : GedcomItem
    {
		public string Name { get; set; }
		public string Address { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }

		public override string ToString()
		{
			var toString = string.Format("{0}: {1}", ID, Name);
			return toString;
		}
	}
}
