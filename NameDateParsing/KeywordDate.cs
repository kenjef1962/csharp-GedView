
namespace NameDateParsing
{
    public enum DateKeyword : int
    {
        BIC,
        Cancelled,
        Child,
        Cleared,
        Completed,
        Dead,
        Deceased,
        Done,
        DNS,
        DNS_CAN,
        Infant,
        NeverMarried,
        Pre_1970,
        Stillborn,
        Submitted,
        Uncleared,
        Young,
        Unknown,
        Private,
        NotMarried

    }

    /// <summary>
    /// Represents a date as a keyword
    /// <remarks>
    /// Date Encoding:
    /// 1 0 0 0 | 0 0 0 0 | 0 0 0 0 | 0 0 0 0 || 0 0 0 0 | 0 0 0 0 | 0 0 0 0 | 0 0 0 0
    /// | \__________________________________________________________________________/
    ///                                                             
    /// E                                   Keyword
    ///                                     
    /// E - Encoding type (always 1)
    /// Keyword - one of a list of possible date values (such as STILLBORN)
    /// </remarks>
    /// </summary>
    public class KeywordDate : Date
    {
        internal KeywordDate(uint code)
            : base(code)
        {
        }

        public KeywordDate(DateKeyword keyword)
            : base(Encode(keyword))
        {
        }

        internal override uint SortDate
        {
            get { return 0xFFFFFFFF; }
        }

        public static uint Encode(DateKeyword keyword)
        {
            return ENCODING_KEYWORD | (uint)keyword;
        }

        public DateKeyword Keyword
        {
            get
            {
                return (DateKeyword)(this._code & KEYWORD_MASK);
            }
        }

        public override bool Equals(Date date)
        {
            if (!(date is KeywordDate)) return false;

            return _code == ((KeywordDate)date)._code;
        }
    }
}
