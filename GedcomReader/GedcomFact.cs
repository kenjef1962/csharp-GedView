using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace GedcomReader
{
    public class GedcomFact : GedcomItem, IComparable<GedcomFact>
    {
		public string Type { get; set; }
		public GedcomDate Date { get; set; }
        public GedcomPlace Place { get; set; }
		public string Description { get; set; }

		public bool IsLDS { get; set; }
		public bool IsShared { get; set; }
		public string Xref { get; set; }

        public override string ToString()
		{
            var toString = string.Format("{0}: {1} {2} {3}", 
                                         ID,
                                         Type,
                                         (Date == null) ? "<no date>" : Date.ToString(), 
                                         (Place == null) ? "<no place>" : Place.ToString());

            return toString;
		}

        public int CompareTo(GedcomFact fact)
        {
            if ((Date != null) && (fact.Date != null))
            {
                return Date.CompareTo(fact.Date);

                // Compare types when dates are equal
                // Birth < Chr or Bapt
                // Death < Burial
                // Marriage < Divorce
            }

            if ((Date != null) && (fact.Date == null))
            {
                return -1;
            }

            if ((Date == null) && (fact.Date != null))
            {
                return 1;
            }

            return 0;
        }
	}
}
