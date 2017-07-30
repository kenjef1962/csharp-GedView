using System.Collections.ObjectModel;

namespace GedcomReader
{
    public class GedcomFamily : GedcomItem
    {
		public string HusbandID { get; set; }
		public string WifeID { get; set; }
		public ObservableCollection<string> ChildrenIDs { get; set; }
		public ObservableCollection<GedcomFact> Facts { get; set; }
		public string RefNo { get; set; }
		public string Rin { get; set; }

		public GedcomFamily()
        {
			ChildrenIDs = new ObservableCollection<string>();
			Facts = new ObservableCollection<GedcomFact>();
		}

		public override string ToString()
		{
			var toString = string.Format("{0}: H: {1} W: {2}", ID, HusbandID, WifeID);
			return toString;
		}

		public void AddChild(string childID)
		{
			ChildrenIDs.Add(childID);
		}

		public void AddFact(GedcomFact fact)
		{
			Facts.Add(fact);
		}
	}
}
