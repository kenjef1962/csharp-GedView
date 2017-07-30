using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace GedViewWPF.Model
{
    public class Citation : IComparable<Citation>
    {
        public string ID { get; set; }
        public string SourceID { get; set; }
        public string SourceTitle { get; set; }
        public string Page { get; set; }
        public string Text { get; set; }
        public string Quality { get; set; }

        public int MediaCount { get; set; }
        public int NoteCount { get; set; }

        public string MediaFilename { get; set; }
        public Visibility MediaVisibility 
        { 
            get
            {
                return string.IsNullOrEmpty(MediaFilename) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

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

        public int CompareTo(Citation citation)
        {
            // Compare Source Title
            if ((SourceTitle != null) && (citation.SourceTitle != null))
                return SourceTitle.CompareTo(citation.SourceTitle);

            if ((SourceTitle != null) && (citation.SourceTitle == null))
                return -1;

            if ((SourceTitle == null) && (citation.SourceTitle != null))
                return 1;

            // Compare Pages
            if ((Page != null) && (citation.Page != null))
                return Page.CompareTo(citation.Page);

            if ((Page != null) && (citation.Page == null))
                return -1;

            if ((Page == null) && (citation.Page != null))
                return 1;

            return 0;
        }
    }
}
