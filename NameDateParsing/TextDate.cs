using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NameDateParsing
{
    public class TextDate : Date
    {
        string _text;

        internal TextDate(string text)
            : base(null)
        {
            _text = text;
        }

        internal override uint SortDate
        {
            get { return 0xFFFFFFFF; }
        }

        public string Text
        {
            get { return _text; }
        }

        public override bool Equals(Date date)
        {
            if (!(date is TextDate)) return false;

            TextDate dt = (TextDate)date;

            return _text == dt._text;
        }
    }
}
