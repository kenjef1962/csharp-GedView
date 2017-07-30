using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedViewWPF.Model
{
    public class Clue : IComparable<Clue> 
    {
        public string Category { get; set; }
        public string Age { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SearchString { get; set; }
        public string Url { get; set; }

        public override string ToString()
        {
            var toString = string.Empty;
            return toString;
        }

        public int CompareTo(Clue clue)
        {
            return 0;
        }
    }
}
