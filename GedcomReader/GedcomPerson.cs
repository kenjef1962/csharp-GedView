using System;
using System.Collections.ObjectModel;

namespace GedcomReader
{
    public class GedcomPerson : GedcomItem, IComparable<GedcomPerson>
    {
        public string Sex { get; set; }
		public ObservableCollection<GedcomPersonName> Names { get; set; }
		public ObservableCollection<GedcomFact> Facts { get; set; }
		public string RefNo { get; set; }
		public string Rin { get; set; }

		public GedcomPersonName Name
		{
			get { return (0 < Names.Count) ? Names[0] : null; }
		}

        public GedcomPerson()
        {
			Names = new ObservableCollection<GedcomPersonName>();
			Facts = new ObservableCollection<GedcomFact>();
		}

		public override string ToString()
		{
			var toString = (Name == null) ? "<no name>" : Name.ToString();
			return toString.Trim();
		}

        public int CompareTo(GedcomPerson person)
        {
            if ((0 < Names.Count) && (0 < person.Names.Count))
            {
                return Names[0].CompareTo(person.Names[0]);
            }
            
            if ((0 < Names.Count) && (0 == person.Names.Count))
            {
                return -1;
            }

            if ((0 == Names.Count) && (0 < person.Names.Count))
            {
                return 1;
            }

            return 0;
        }

		public void AddName(GedcomPersonName name)
		{
			Names.Add(name);			
		}

		public void AddFact(GedcomFact fact)
		{
			Facts.Add(fact);
		}
	}
}
