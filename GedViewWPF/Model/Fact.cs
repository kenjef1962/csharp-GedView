using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedViewWPF.Model
{
    public class Fact : IComparable<Fact> 
    {
        public string ID { get; set; }
        public string Type { get; set; }
        public Date Date { get; set; }
        public string Place { get; set; }
        public string Description { get; set; }

        public string Age { get; set; }

        public int CitationCount { get; set; }
        public int MediaCount { get; set; }
        public int NoteCount { get; set; }

        public List<string> PersonIDs { get; set; }
        public List<string> FamilyIDs { get; set; }

        public Fact() 
        {
            PersonIDs = new List<string>();
            FamilyIDs = new List<string>();
        }

        public override string ToString()
        {
            var toString = string.Empty;

            if (Date != null)
            {
                toString += Date.ToString();
            }

            if (!string.IsNullOrEmpty(Place))
            {
                if (!string.IsNullOrEmpty(toString))
                    toString += " ";

                toString += string.Format("in {0}", Place);
            }

            if (!string.IsNullOrEmpty(Description))
            {
                if (!string.IsNullOrEmpty(toString))
                    toString += " ";

                toString += string.Format("[{0}]", Description);
            }

            return toString;
        }

        public int CompareTo(Fact fact)
        {
            if ((Date != null) && (fact.Date != null))
                return Date.CompareTo(fact.Date);

            if ((Date != null) && (fact.Date == null))
                return -1;

            if ((Date == null) && (fact.Date != null))
                return 1;

            return 0;
        }
    }
}
