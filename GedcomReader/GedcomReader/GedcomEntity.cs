using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomEntity
    {
        public string ID { get; set; }
		public string RIN { get; set; }
		public List<string> ReferenceNumbers { get; set; }
		public List<GedcomCitation> Citations { get; set; }
        public List<GedcomMedia> Media { get; set; }
        public List<GedcomNote> Notes { get; set; }
		public GedcomChange ChangeDate { get; set; }

        public GedcomEntity()
        {
			ReferenceNumbers = new List<string>();
            Citations = new List<GedcomCitation>();
            Media = new List<GedcomMedia>();
            Notes = new List<GedcomNote>();
        }
    }
}
