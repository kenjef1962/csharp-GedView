using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedViewWPF.Model
{
    public class Date : IComparable<Date>
    {
        public string DateStr { get; set; }
        public DateTime? DateDT { get; set; }
        public int Modifier { get; set; }

        public int? Year
        {
            get
            {
                if (DateDT != null)
                {
                    return ((DateTime)DateDT).Year;
                }

                return null;
            }
        }

        public override string ToString()
        {
            return DateStr;
        }

        public int CompareTo(Date date)
        {
            if ((DateDT != null) && (date.DateDT != null))
            {
                var dt1 = (DateTime)DateDT;
                var dt2 = (DateTime)date.DateDT;
                var result = dt1.CompareTo(dt2);

                if (result == 0)
                {
                    return Modifier.CompareTo(date.Modifier);
                }

                return result;
            }
            else if ((DateDT != null) && (date.DateDT == null))
            {
                return -1;
            }
            else if ((DateDT == null) && (date.DateDT != null))
            {
                return 1;
            }

            return 0;
        }
    }
}
