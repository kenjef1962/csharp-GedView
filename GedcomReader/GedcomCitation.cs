
using System;
namespace GedcomReader
{
    public class GedcomCitation : GedcomItem, IComparable<GedcomCitation>
    {
		public string SourceID { get; set; }
        public string SourceTitle { get; set; }
        public string Page { get; set; }
        public string Text { get; set; }
		public string Quality { get; set; }

		public override string ToString()
		{
			var toString = SourceTitle;

            if (!string.IsNullOrEmpty(Page))
            {
                if (!string.IsNullOrEmpty(toString))
                    toString += "; ";

                toString += Page;
            }

            if (!string.IsNullOrEmpty(Text))
            {
                if (!string.IsNullOrEmpty(toString))
                    toString += "; ";

                toString += Text;
            }

			return toString;
		}

        public int CompareTo(GedcomCitation citation)
        {
            var result = SourceTitle.CompareTo(citation.SourceTitle);

            if (result == 0)
            {
                result = Page.CompareTo(citation.Page);
            }

            return result;
        }
	}
}
