using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace GedcomReader
{
    public class GedcomDate : GedcomItem, IComparable<GedcomDate>
    {
		public string DateStr { get; set; }
        public DateTime? Date { get; set; }
        public int Modifier { get; set; }

		public override string ToString()
		{
            return DateStr;
		}

        public int CompareTo(GedcomDate date)
        {
            if ((Date != null) && (date.Date != null))
            {
                var dt1 = (DateTime)Date;
                var dt2 = (DateTime)date.Date;
                var result = dt1.CompareTo(dt2);

                if (result == 0)
                {
                    return Modifier.CompareTo(date.Modifier);
                }

                return result;
            }
            else if ((Date != null) && (date.Date == null))
            {
                return -1;
            }
            else if ((Date == null) && (date.Date != null))
            {
                return 1;
            }

            return 0;
        }
	}
}
