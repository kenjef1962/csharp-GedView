using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedViewWPF.Model
{
    public class GroupHeader
    {
        public string Header { get; set; }
        public Person HeaderPerson { get; set; }

        public override string ToString()
        {
            return Header;
        }
    }
}
