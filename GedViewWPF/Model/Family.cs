using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedViewWPF.Model
{
    public class Family
    {
        public string ID { get; set; }
        public Person Husband { get; set; }
        public Person Wife { get; set; }
        public Fact Marriage { get; set; }
        public List<Person> Children { get; set; }
    }
}
