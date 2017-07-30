using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class GedcomFactEntity : GedcomEntity
    {
        public List<GedcomFact> Facts { get; set; }

        public GedcomFactEntity()
        {
            Facts = new List<GedcomFact>();
        }
    }
}
