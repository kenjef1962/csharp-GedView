using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedViewWPF.Model
{
    public class Source : IComparable<Source>
    {
        public string ID { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return Title;
        }

        public int CompareTo(Source source)
        {
            var result = Title.CompareTo(source.Title);
            return result;
        }
    }
}
