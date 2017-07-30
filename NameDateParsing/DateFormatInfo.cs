using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;

namespace NameDateParsing
{
    /// <summary>
    /// Handles formatting for dates
    /// </summary>
    public class DateFormatInfo : FormatInfo
    {
        CultureInfo _culture;
        private char[] _delimiters = {',', '/', '-', '.'};
        protected static string _BC;        
        protected static string _AD;
        protected static int _ADCutoffYear = 999;
        protected static string _fuzzinessText;
        protected bool _suppressModifier = false;

        public const string DMonY = "DMY"; // DO NOT TRANSLATE
        public const string DMonthY = "LDMY"; // DO NOT TRANSLATE
        public const string MonDY = "MDY"; // DO NOT TRANSLATE
        public const string MonDDY = "MDDY"; // DO NOT TRANSLATE
        public const string MonthDY = "LMDY"; // DO NOT TRANSLATE
        public const string MonthDDY = "LMDDY"; // DO NOT TRANSLATE
        public const string Year = "yyyy"; // DO NOT TRANSLATE
        public const string Month = "m"; // DO NOT TRANSLATE
        public const string LZMonth = "mm"; // DO NOT TRANSLATE
        public const string MonStr = "Mmm"; // DO NOT TRANSLATE
        public const string MonthStr = "Mmmm"; // DO NOT TRANSLATE
        public const string Day = "d"; // DO NOT TRANSLATE
        public const string LZDay = "dd"; // DO NOT TRANSLATE
        public const string Modifier = "mod"; // DO NOT TRANSLATE
        public const string Standard = DMonY;

        public static readonly DateFormatInfo Default = new DateFormatInfo();

        public int ADCutoffYear
        {
            get
            {
                return _ADCutoffYear;
            }
            set
            {
                _ADCutoffYear = value;
            }
        }

        public bool CommonEra
        {
            set 
            {
                DateParser.CommonEra = value;

				_BC = DateParser.CommonEra ? Properties.Resources.DateBCE : Properties.Resources.DateBC;
				_AD = DateParser.CommonEra ? Properties.Resources.DateCE : Properties.Resources.DateAD;
            }
        }

        public string FuzzinessText
        {
            get
            {
                return _fuzzinessText;
            }
            set
            {
                _fuzzinessText = value;
            }
        }

        public CultureInfo Culture
        {
            get
            {
                return _culture;
            }
            set
            {
                _culture = value;
                Properties.Resources.Culture = _culture;
            }
        }

        public bool SuppressModifier
        {
            get { return _suppressModifier; }
            set { _suppressModifier = value; }
        }

        public DateFormatInfo()
        {
            Initialize(Thread.CurrentThread.CurrentUICulture);
        }

        public DateFormatInfo(CultureInfo culture)
        {
            Initialize(culture);
        }

        public DateFormatInfo(string defaultFormat)
            : base(defaultFormat)
        {
            Initialize(Thread.CurrentThread.CurrentUICulture);
        }

        private void Initialize(CultureInfo culture)
        {
            Culture = culture;
            CommonEra = false;
        }

        protected override string GetCacheKey(object obj)
        {
            if (obj is IRange<Date>)
            {
                IRange<Date> range = (IRange<Date>)obj;
                return string.Format("{0}_{1}{2}", _culture.LCID, Date.Encode(range.Begin), Date.Encode(range.End));
            }
            else if (obj is TextDate)
            {
                return ((TextDate)obj).Text;
            }
            else if (obj is Date)
            {
                Date dt = (Date)obj;
                uint? code = Date.Encode(dt);

                return string.Format("{0}_{1}", _culture.LCID, code.ToString());
            }
            else
            {
                return base.GetCacheKey(obj);
            }
        }

        protected override string InnerFormat(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is TextDate)
            {
                return ((TextDate)arg).Text;
            }
            else if (arg is KeywordDate)
            {
                return FormatKeyword(((KeywordDate)arg).Keyword);
            }
            else if (arg is Date)
            {
                return FormatDate(format, (Date)arg, formatProvider);
            }

            return base.InnerFormat(format, arg, formatProvider);
        }

        private string FormatDate(string format, Date date, IFormatProvider formatProvider)
        {
            int idx = format.IndexOfAny(_delimiters);
            char delimiter = ' ';
            if (idx >= 0)
            {
                delimiter = format[idx];
            }

            // This is a special case where we want to grab the year and not allow default date range formatting to come into play
            if (date is DateRange && format == Year)
            {
                date = ((DateRange) date).Begin;
            }

            if (date is DateRange)
            {
                DateRange range = (DateRange)date;
                int? beginYear = range.Begin.Year;
                int? endYear = range.End.Year;
                int? beginMonth = range.Begin.Month;
                int? endMonth = range.End.Month;
                int? beginDay = range.Begin.Day;
                int? endDay = range.End.Day;
                DateModifier beginModifier = range.Begin.Modifier;
                DateModifier endModifier = range.End.Modifier;

                if (((beginModifier & DateModifier.DoubleDate) == 0) && beginYear == endYear)
                {
                    beginYear = null;
                    beginModifier &= ~DateModifier.YearMissing;

                    if (beginMonth == endMonth)
                    {
                        string tempFormat = format.Replace("mod", "");
                        int im = tempFormat.IndexOf("m");
                        int id = tempFormat.IndexOf("d");
                        // if month is to be displayed first then strip it from the end part, otherwise strip it from the begin part
                        if (format == MonDY || format == MonDDY || format == MonthDY || format == MonthDDY || (im < id))
                        {
                            endMonth = null;
                            endModifier &= ~DateModifier.MonthMissing;
                        }
                        else
                        {
                            beginMonth = null;
                            beginModifier &= ~DateModifier.MonthMissing;
                        }
                    }
                }

                Date begin = new SDNDate(SDNDate.Encode(beginYear, beginMonth, beginDay, beginModifier));
                Date end = new SDNDate(SDNDate.Encode(endYear, endMonth, endDay, endModifier));
                string dateRangeFormat;
                if ( this._suppressModifier )
                {
                    dateRangeFormat = "{{0:{0}}}–{{1:{0}}}"; // DO NOT TRANSLATE
                }
                else
                {
                    dateRangeFormat = Properties.Resources.DateRangeFormat;
                }

                // embed the format string into the range format string
                dateRangeFormat = string.Format(dateRangeFormat, format);

                return string.Format(formatProvider, dateRangeFormat, begin, end);
            }
            if (date.Year == null && date.Month == null && date.Day == null)
            {
                return string.Empty;
            }

            string ret = string.Empty;
            switch (format)
            {
                case General:
                case DMonY: // [[dd ]Mmm ][yyyy]
                    ret = string.Format("{0} {1} {2} {3}", // DO NOT TRANSLATE
                        FormatModifier(date.Modifier),
                        FormatDay(date, "d2"), // DO NOT TRANSLATE
                        FormatMonthStrAbbrev(date, _culture),
                        FormatFullYear(date));
                    //ret = string.Format(formatProvider, "{0:mod dd Mmm yyyy}", arg);
                    break;
                case DMonthY: // [[dd ]Mmmm ][yyyy]
                    ret = string.Format("{0} {1} {2} {3}", // DO NOT TRANSLATE
                        FormatModifier(date.Modifier),
                        FormatDay(date, "d2"), // DO NOT TRANSLATE
                        FormatMonthStr(date, _culture),
                        FormatFullYear(date));
                    //ret = string.Format(formatProvider, "{0:mod dd Mmmm yyyy}", arg);
                    break;
                case MonDY: // [Mmm[ d], ]yyyy
                    {
                        if (date.HasMonth() && date.HasDay() && date.HasYear())
                        {
                            // use the specialized american format with a comma between the month name and the day
                            ret = string.Format(formatProvider, "{0:mod Mmm d, yyyy}", date); // DO NOT TRANSLATE
                        }
                        else
                        {
                            ret = string.Format(formatProvider, "{0:mod Mmm d yyyy}", date); // DO NOT TRANSLATE
                        }
                    }
                    break;
                case MonDDY: // [Mmm[ dd], ]yyyy
                    {
                        if (date.HasMonth() && date.HasDay() && date.HasYear())
                        {
                            // use the specialized american format with a comma between the month name and the day
                            ret = string.Format(formatProvider, "{0:mod Mmm dd, yyyy}", date); // DO NOT TRANSLATE
                        }
                        else
                        {
                            ret = string.Format(formatProvider, "{0:mod Mmm dd yyyy}", date); // DO NOT TRANSLATE
                        }
                    }
                    break;
                case MonthDY: // [Mmmm[ d], ]yyyy
                    if (date.HasMonth() && date.HasDay() && date.HasYear())
                    {
                        // use the specialized american format with a comma between the month name and the day
                        ret = string.Format(formatProvider, "{0:mod Mmmm d, yyyy}", date); // DO NOT TRANSLATE
                    }
                    else
                    {
                        ret = string.Format(formatProvider, "{0:mod Mmmm d yyyy}", date); // DO NOT TRANSLATE
                    }
                    break;
                case MonthDDY: // [Mmmm[ dd], ]yyyy
                    if (date.HasMonth() && date.HasDay() && date.HasYear())
                    {
                        // use the specialized american format with a comma between the month name and the day
                        ret = string.Format(formatProvider, "{0:mod Mmmm dd, yyyy}", date); // DO NOT TRANSLATE
                    }
                    else
                    {
                        ret = string.Format(formatProvider, "{0:mod Mmmm dd yyyy}", date); // DO NOT TRANSLATE
                    }
                    break;
                default:
                    ret = base.InnerFormat(format, date, formatProvider);
                    break;
            }
            ret = base.CollapseWhitespace(ret);
            ret = CollapseDelimiters(delimiter, ret);
            ret = ret.TrimEnd(delimiter);

            // remove delimiter(s) at the beginning
            //ret = ret.TrimStart(_delimiters);


            //ret = Regex.Replace(ret, string.Format(@"\s\{0}", delimiter), delimiter.ToString()); // remove spaces before the delimiter
            //ret = Regex.Replace(ret, string.Format(@"\{0}\{0}", delimiter), delimiter.ToString()); // remove duplicate delimiters

            return ret;
        }

        private string FormatModifier(DateModifier modifier)
        {
            if (_suppressModifier) return string.Empty;

            switch (((int)modifier & 0x0103))
            {
                case (int)DateModifier.None: return string.Empty;
                case (int)DateModifier.About: return _fuzzinessText ?? Properties.Resources.DateModifierAbout;
                case (int)DateModifier.After: return Properties.Resources.DateModifierAfter;
                case (int)DateModifier.Before: return Properties.Resources.DateModifierBefore;
                case (int)DateModifier.Calculated: return Properties.Resources.DateModifierCalculated;
                default: return string.Empty;
            }
        }

        private static string FormatKeyword(DateKeyword keyword)
        {
            switch (keyword)
            {
                case DateKeyword.BIC: return Properties.Resources.DateKeywordBIC; 
                case DateKeyword.Cancelled: return Properties.Resources.DateKeywordCancelled; 
                case DateKeyword.Child: return Properties.Resources.DateKeywordChild; 
                case DateKeyword.Cleared: return Properties.Resources.DateKeywordCleared; 
                case DateKeyword.Completed: return Properties.Resources.DateKeywordCompleted; 
                case DateKeyword.Dead: return Properties.Resources.DateKeywordDead; 
                case DateKeyword.Deceased: return Properties.Resources.DateKeywordDeceased; 
                case DateKeyword.DNS: return Properties.Resources.DateKeywordDNS; 
                case DateKeyword.DNS_CAN: return Properties.Resources.DateKeywordDNSCAN; 
                case DateKeyword.Done: return Properties.Resources.DateKeywordDone; 
                case DateKeyword.Infant: return Properties.Resources.DateKeywordInfant; 
                case DateKeyword.NeverMarried: return Properties.Resources.DateKeywordNeverMarried;
                case DateKeyword.NotMarried: return Properties.Resources.DateKeywordNotMarried; 
                case DateKeyword.Pre_1970: return Properties.Resources.DateKeywordPre1970; 
                case DateKeyword.Stillborn: return Properties.Resources.DateKeywordStillborn; 
                case DateKeyword.Submitted: return Properties.Resources.DateKeywordSubmitted; 
                case DateKeyword.Uncleared: return Properties.Resources.DateKeywordUncleared;
                case DateKeyword.Young: return Properties.Resources.DateKeywordYoung;
                case DateKeyword.Unknown: return Properties.Resources.DateKeywordUnknown;
                case DateKeyword.Private: return Properties.Resources.DateKeywordPrivate;
                default: return Properties.Resources.DateKeywordUnknown; // DO NOT TRANSLATE
            }
        }

        private string FormatDay(IDate date, string format)
        {
            if (date.Day != null && (date.Modifier & DateModifier.DayMissing) != DateModifier.DayMissing)
            {
                return ((int)date.Day).ToString(format);
            }

            return string.Empty;
        }

        private string FormatMonthStrAbbrev(IDate date, CultureInfo culture)
        {
            if (date.Month != null && date.Month > 0 && (date.Modifier & DateModifier.MonthMissing) != DateModifier.MonthMissing)
            {
                string retVal = "";
                retVal = culture.DateTimeFormat.AbbreviatedMonthNames[((byte)date.Month) - 1];
                if ((date.Modifier & DateModifier.Quarter) == DateModifier.Quarter)
                {
                    retVal += " Qtr"; // DO NOT TRANSLATE
                }
                return retVal;
            }

            return string.Empty;
        }

        private string FormatMonthStr(IDate date, CultureInfo culture)
        {
            if (date.Month != null && (date.Modifier & DateModifier.MonthMissing) != DateModifier.MonthMissing)
            {
                string retVal = "";
                retVal = culture.DateTimeFormat.MonthNames[((byte)date.Month) - 1];
                if ((date.Modifier & DateModifier.Quarter) == DateModifier.Quarter)
                {
                    retVal += " Qtr"; // DO NOT TRANSLATE
                }
                return retVal;
            }

            return string.Empty;
        }

        private string FormatMonth(IDate date, string format)
        {
            if (date.Month != null && (date.Modifier & DateModifier.MonthMissing) != DateModifier.MonthMissing)
            {
                string retVal = ((int)date.Month).ToString(format); ;
                if ((date.Modifier & DateModifier.Quarter) == DateModifier.Quarter)
                {
                    retVal += " Qtr"; // DO NOT TRANSLATE
                }
                return retVal;
            }

            return string.Empty;
        }

        private string FormatFullYear(IDate date) 
        {
            string result = string.Empty;

            if (date.Year != null && (date.Modifier & DateModifier.YearMissing) != DateModifier.YearMissing)
            {
                if ((date.Modifier & DateModifier.DoubleDate) == DateModifier.DoubleDate)
                {
                    int year2 = Math.Abs((int)date.Year);
                    int year1 = year2 - 1;

                    string year2Str = year2 > 9 ? year2.ToString("d2") : year2.ToString(); // DO NOT TRANSLATE
                    if (year2Str.Length > 2)
                        year2Str = year2Str.Substring(year2Str.Length - 2);

                    result = string.Format("{0}/{1} {2}",  // DO NOT TRANSLATE
                        year1, 
                        year2Str,
                        ((date.Year < 0) ? _BC : ((date.Year <= _ADCutoffYear) ? _AD : string.Empty))).Trim();
                }
                else
                {
                    int year = Math.Abs((int)date.Year);

                    result = string.Format("{0} {1}", // DO NOT TRANSLATE
                        year,
                        ((date.Year < 0) ? _BC : ((date.Year <= _ADCutoffYear) ? _AD : string.Empty))).Trim();
                }
            }

            return result;
        }

        protected override CustomFormatter  GetCustomFormatter(object arg)
        {
            return new MyCustomFormatter((IDate)arg, this);
        }

        private class MyCustomFormatter : CustomFormatter
        {
            protected IDate _date;
            protected DateFormatInfo _outer;

            public MyCustomFormatter(IDate date, DateFormatInfo outer)
            {
                _outer = outer;
                _date = date;
            }

            public override string ReplaceTerm(string term)
            {
                switch (term)
                {
                    case Modifier: // modifier
                        return _outer.FormatModifier(_date.Modifier);
                    case Day: // day with no leading zero
                        return _outer.FormatDay(_date, "d"); // DO NOT TRANSLATE
                    case LZDay: // day with leading zero
                        return _outer.FormatDay(_date, "d2"); // DO NOT TRANSLATE
                    case MonStr: // abbreviated name of month (Jan, Feb, etc)
                        return _outer.FormatMonthStrAbbrev(_date, _outer._culture);
                    case MonthStr: // full name of month (January, February, etc)
                        return _outer.FormatMonthStr(_date, _outer._culture);
                    case Month: // month without leading zero
                        return _outer.FormatMonth(_date, "d"); // DO NOT TRANSLATE
                    case LZMonth: // two digit month 01-12
                        return _outer.FormatMonth(_date, "d2"); // DO NOT TRANSLATE
                    case Year: // four digit year
                        return _outer.FormatFullYear(_date);
                    default: return base.ReplaceTerm(term);
                }
            }
        }

        private string CollapseDelimiters(char delimiter, string s)
        {
            string pattern = string.Format(@"([^\s\{0}]*)(\{0})+", delimiter); // DO NOT TRANSLATE
            return Regex.Replace(s, pattern, EvaluateDelimiterReplacement).Trim();
        }

        private string EvaluateDelimiterReplacement(Match match)
        {
            if (match.Groups[1].Value == string.Empty)
            {
                return string.Empty;
            }
            else
            {
                return string.Format("{0}{1}", match.Groups[1], match.Groups[2]); // DO NOT TRANSLATE
            }
        }
    }
}
