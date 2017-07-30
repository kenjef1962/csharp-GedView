using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace GedcomReader
{
    public class GedcomPlace : GedcomItem
    {
		public string Place { get; set; }
		public string Form { get; set; }

		public override string ToString()
		{
			return Place;
		}
	}
}
