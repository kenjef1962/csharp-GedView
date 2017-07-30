using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedViewWPF.Model
{
    public class Media : IComparable<Media>
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Filename { get; set; }
        public string Format { get; set; }

        public override string ToString()
        {
            var toString = !string.IsNullOrEmpty(Title) ? Title : Filename;
            return toString;
        }

        public int CompareTo(Media media)
        {
            var result = Title.CompareTo(media.Title);

            if (result == 0)
            {
                result = Filename.CompareTo(media.Filename);
            }

            return result;
        }
    }
}
