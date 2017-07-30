
using System;
namespace GedcomReader
{
    public class GedcomPersonName : GedcomItem, IComparable<GedcomPersonName>
    {
        public string Prefix { get; set; }
        public string Given { get; set; }
        public string Nickname { get; set; }
        public string SurnamePrefix { get; set; }
        public string Surname { get; set; }
        public string Suffix { get; set; }

		public override string ToString()
		{
            var toString = Surname;

            if (!string.IsNullOrEmpty(Given))
            {
                if (!string.IsNullOrEmpty(toString))
                    toString += ", ";

                toString += Given;
            }

            if (!string.IsNullOrEmpty(Suffix))
            {
                if (!string.IsNullOrEmpty(toString))
                    toString += " ";

                toString += "(" + Suffix + ")";
            }

            return toString.Trim();
		}

        public int CompareTo(GedcomPersonName name)
        {
            return this.ToString().CompareTo(name.ToString());
        }
	}
}
