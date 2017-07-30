using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomLogItem
    {
        public string Message { get; set; }
        public int LineNo { get; set; }
        public List<GedcomLine> Lines { get; set; }

        public GedcomLogItem(string message, GedcomLine line)
        {
            Message = message;
            LineNo = line.LineNo;
            Lines = new List<GedcomLine>() { line };
        }
    }
}
