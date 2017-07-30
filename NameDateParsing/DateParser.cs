using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace NameDateParsing
{
    #region SupportClassesAndTypes
    // Date parse errors should be added to either the
    // critical group or non-critical group and should be
    // ordered according to their priority.  Errors returned
    // from the date parser will be returned in the reverse
    // order of this list (i.e. highest to lowest)

    public enum DateParseErrorType
    {
        None,
        // NON-CRITICAL
        TwoDigitYear, 
        OneDigitYear, 
        InvalidDoubleDate,
        DoubleDateAmbiguous,
        FutureDate,
        WrongAncientDateFormat,
        // CRITICAL
        DayOutOfRange,
        MonthOutOfRange, 
        YearOutOfRange,
        NotLeapYear,
        Unknown,
        UnknownContent,
        RangeError
    }

    
    public class DateParseErrorInfo
    {
        public readonly DateParseErrorType Reason;
        public readonly string Data;
        public readonly bool SecondDateError = false;
        public DateParseErrorInfo(DateParseErrorType reason, string data, bool secondDateError)
        {
            Reason = reason;
            Data = data;
            SecondDateError = secondDateError;
        }
    }


    public class DateParseError
    {
        public enum SeverityLevel
        {
            Warning,
            Critical
        }

        public readonly uint? FirstDate;
        public readonly uint? SecondDate;
        public readonly IList<DateParseErrorInfo> ErrorInfo;

        public DateParseError(IList<DateParseErrorInfo> errorInfo)
        {
            ErrorInfo = errorInfo;
        }

        public DateParseError(IList<DateParseErrorInfo> errorInfo, uint? firstDate, uint? secondDate)
        {
            ErrorInfo = errorInfo;
            FirstDate = firstDate;
            SecondDate = secondDate;
        }


        protected bool isCritical(DateParseErrorInfo errorInfo)
        {
            switch (errorInfo.Reason)
            {
                case DateParseErrorType.TwoDigitYear:
                case DateParseErrorType.OneDigitYear:
                case DateParseErrorType.DoubleDateAmbiguous:
                case DateParseErrorType.FutureDate:
                case DateParseErrorType.WrongAncientDateFormat:
                    break; // do nothing -- it's a warning
                // TODO: decide which errors are Warnings vs. Critical errors
                case DateParseErrorType.InvalidDoubleDate:
                case DateParseErrorType.Unknown:
                case DateParseErrorType.MonthOutOfRange:
                case DateParseErrorType.DayOutOfRange:
                case DateParseErrorType.UnknownContent:
                case DateParseErrorType.RangeError:
                case DateParseErrorType.YearOutOfRange:
                case DateParseErrorType.NotLeapYear:
                default:
                    return true;
            }
            return false;
        }



        protected int DefaultErrorIndex
        {
            get
            {
                int index = 0;
                for (int i = 0; i < ErrorInfo.Count; i++)
                {
                    if (isCritical(ErrorInfo[i]))
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }
        }

            public DateParseErrorType Reason
        {
            get { return ErrorInfo[DefaultErrorIndex].Reason; }
        }

        public string Data
        {
            get { return ErrorInfo[DefaultErrorIndex].Data.Trim(); }
        }

        public SeverityLevel Severity
        {
            get
            {
                foreach (DateParseErrorInfo info in ErrorInfo)
                {
                    if (isCritical(info))
                        return SeverityLevel.Critical;
                }
                return SeverityLevel.Warning;
            }
        }

        public string DebugAllErrors
        {
            get
            {
                string errorInfo = ""; // DO NOT TRANSLATE
                foreach (DateParseErrorInfo error in ErrorInfo)
                    errorInfo += error.Reason + " -> " + error.Data + "\r\n"; // DO NOT TRANSLATE
                return errorInfo;
            }
        }
    }

    public class DateParseException : Exception
    {
        private DateParseError _error;

        internal DateParseException(DateParseError error)
        {
            _error = error;
        }

        internal DateParseException(DateParseError error, string msg)
            : base(msg)
        {
            _error = error;
        }

        internal DateParseException(DateParseError error, string msg, Exception innerException)
            : base(msg, innerException)
        {
            _error = error;
        }

        public DateParseError Error
        {
            get
            {
                return _error;
            }
        }

        public uint? FirstDate
        {
            get { return _error.FirstDate; }
        }

        public uint? SecondDate
        {
            get { return _error.SecondDate; }
        }
    }


    public class ParseData
    {
        string _dateStr;
        uint? _firstDate;
        uint? _secondDate;
        DateModifier _qualifier;
        public bool ParsingSecondDate = false;

        IList<DateParseErrorInfo> _dateParseErrorInfo = new List<DateParseErrorInfo>();

        internal ParseData(string dateStr, uint? firstDate, uint? secondDate)
        {
            _dateStr = dateStr;
            _firstDate = firstDate;
            _secondDate = secondDate;
        }

        internal string DateStr
        {
            get { return _dateStr; }
            set { _dateStr = value; }
        }

        internal uint? FirstDate
        {
            get { return _firstDate; }
            set { _firstDate = value; }
        }

        internal uint? SecondDate
        {
            get { return _secondDate; }
            set { _secondDate = value; }
        }

        internal DateModifier Qualifier
        {
            get { return _qualifier; }
            set { _qualifier = value; }
        }

        internal IList<DateParseErrorInfo> DateParseErrorInfo
        {
            get { return _dateParseErrorInfo; }
        }

    }
    #endregion

    #region DateParser Class

    public class DateParser
    {
        private static Dictionary<string, Regex> s_parsers;

        private CultureInfo _culture;
        private Regex[] _parsers;

        static DateParser()
        {
            s_parsers = new Dictionary<string, Regex>();
            s_parsers.Add("special0", new Regex("([0-9]+)/([0-9]+)/([0-9]+) - 0/([0-9]+)/0", RegexOptions.Compiled));
            s_parsers.Add("special1", new Regex(@"^ *([0-9]+) *- *([0-9]+)([ -]+\p{L}{3}[ -]+[0-9]+) *$", RegexOptions.Compiled));
            s_parsers.Add("special2", new Regex(@"Or:([0-9][0-9][0-9]*)/([0-9][0-9])/([0-9][0-9])\-([0-9][0-9][0-9]*)/00/00", RegexOptions.Compiled));
            s_parsers.Add("standard", new Regex(@"([0-9]+) (\p{L}+) ([0-9]+)", RegexOptions.Compiled));

            DayEx = "([0-9][0-9]?)";//([0-9]|[0-2][0-9]|3[0-1])"; // DO NOT TRANSLATE
            MonthEx = "([0-9][0-9]?)";//"([0-9]|0[0-9]|1[0-2])"; // DO NOT TRANSLATE
            YearEx = "([0-9]{3,8})"; // DO NOT TRANSLATE
            DoubleYearEx = "([0-9][0-9][0-9]+ */ *[0-9]+)"; // DO NOT TRANSLATE
            TwoDigitDoubleYearEx = "([0-9][0-9] */ *[0-9]+)"; // DO NOT TRANSLATE
            OneDigitDoubleYearEx = "([0-9] */ *[0-9]+)"; // DO NOT TRANSLATE
            TwoYearEx = "([0-9][0-9]?)"; // DO NOT TRANSLATE
            AnyYearEx = "([0-9]+)"; // DO NOT TRANSLATE
            MonthStrEx = @"(\p{L}{3,})"; // DO NOT TRANSLATE
        }

        private const int NUMPARSERS = 44;
        private const int PARSER01 = 44;
        private const int ILLEGAL1 = 45;
        private const int ILLEGAL2 = 46;
        private const int RANGE1 = 47;
        private const int RANGE2 = 48;

        public DateParser(CultureInfo culture)
        {
            _culture = culture;

            Months = new string[,] {
            {_culture.DateTimeFormat.MonthNames[0].ToLower(),_culture.DateTimeFormat.AbbreviatedMonthNames[0].ToLower(), null},
            {_culture.DateTimeFormat.MonthNames[1].ToLower(),_culture.DateTimeFormat.AbbreviatedMonthNames[1].ToLower(), null},
            {_culture.DateTimeFormat.MonthNames[2].ToLower(),_culture.DateTimeFormat.AbbreviatedMonthNames[2].ToLower(), null},
            {_culture.DateTimeFormat.MonthNames[3].ToLower(),_culture.DateTimeFormat.AbbreviatedMonthNames[3].ToLower(), null},
            {_culture.DateTimeFormat.MonthNames[4].ToLower(),_culture.DateTimeFormat.AbbreviatedMonthNames[4].ToLower(), null},
            {_culture.DateTimeFormat.MonthNames[5].ToLower(),_culture.DateTimeFormat.AbbreviatedMonthNames[5].ToLower(), null},
            {_culture.DateTimeFormat.MonthNames[6].ToLower(),_culture.DateTimeFormat.AbbreviatedMonthNames[6].ToLower(), null},
            {_culture.DateTimeFormat.MonthNames[7].ToLower(),_culture.DateTimeFormat.AbbreviatedMonthNames[7].ToLower(), null},
            {_culture.DateTimeFormat.MonthNames[8].ToLower(),_culture.DateTimeFormat.AbbreviatedMonthNames[8].ToLower(), "sept"}, // DO NOT TRANSLATE (sept is a special case in English so we include it here
            {_culture.DateTimeFormat.MonthNames[9].ToLower(),_culture.DateTimeFormat.AbbreviatedMonthNames[9].ToLower(), null},
            {_culture.DateTimeFormat.MonthNames[10].ToLower(),_culture.DateTimeFormat.AbbreviatedMonthNames[10].ToLower(), null},
            {_culture.DateTimeFormat.MonthNames[11].ToLower(),_culture.DateTimeFormat.AbbreviatedMonthNames[11].ToLower(), null}};

            BcRegex = CreatePunctuatedRegExFromResource(DateParserStrings.ResourceManager.GetString("BC", _culture), false);
            BceRegex = CreatePunctuatedRegExFromResource(DateParserStrings.ResourceManager.GetString("BCE", _culture), false);
            AdRegex = CreatePunctuatedRegExFromResource(DateParserStrings.ResourceManager.GetString("AD", _culture), false);
            CeRegex = CreatePunctuatedRegExFromResource(DateParserStrings.ResourceManager.GetString("CE", _culture), false);
            BcAdEx = @" *(" + BceRegex + "|" + BcRegex + "|" + AdRegex + "|" + CeRegex + ")?"; // DO NOT TRANSLATE
            Quarter = "(" + CreateRegExFromResource(DateParserStrings.ResourceManager.GetString("Quarter", _culture)) + ")";

            _parsers = new Regex[]
            {
             /*00 | 01 | 12-22-2000/2001*/     new Regex(@"\b" + MonthEx + @"[-/\., ] *" + DayEx + @"[-/\., ] *" + DoubleYearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                // DO NOT TRANSLATE
             /*01 | 02 | 12-22-2000-2001*/     new Regex(@"\b" + MonthEx + @"[-/\., ] *" + DayEx + @"[-/\., ] *" + YearEx + @"[-]([0-9]+)" + BcAdEx + @"\b", RegexOptions.Compiled),                                     // DO NOT TRANSLATE
             /*02 | 03 | 12-22-2000*/          new Regex(@"\b" + MonthEx + @"[-/\., ] *" + DayEx + @"[-/\., ] *" + YearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                      // DO NOT TRANSLATE
             /*03 | 04 | Dec 22, 20/21*/       new Regex(@"\b" + MonthStrEx + @"[-/\., ]*" + DayEx + @"[-/\., ]+" + TwoDigitDoubleYearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                       // DO NOT TRANSLATE
             /*04 | 05 | Dec 22, 2/3*/         new Regex(@"\b" + MonthStrEx + @"[-/\., ]*" + DayEx + @"[-/\., ]+" + OneDigitDoubleYearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                       // DO NOT TRANSLATE
             /*05 | 06 | 12-22-2000*/          new Regex(@"\b" + MonthEx + @"[-/\., ] *" + DayEx + @"[-/\., ] *" + AnyYearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                   // DO NOT TRANSLATE
             /*06 | 07 | 12-22-20/21*/         new Regex(@"\b" + MonthEx + @"[-/\., ] *" + DayEx + @"[-/\., ] *" + TwoYearEx + @"[-]([0-9]+)" + BcAdEx + @"\b", RegexOptions.Compiled),                                  // DO NOT TRANSLATE
             /*07 | 08 | 12-22-20*/            new Regex(@"\b" + MonthEx + @"[-/\., ] *" + DayEx + @"[-/\., ] *" + TwoYearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                   // DO NOT TRANSLATE
             /*08 | 09 | 2006-01-15*/          new Regex(@"\b" + YearEx + @"[-/\., ] *" + MonthEx + @"[-/\., ] *" + DayEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                      // DO NOT TRANSLATE
             /*09 | 09a | 2006-01*/            new Regex(@"\b" + YearEx + @"[-\., ] *" + MonthEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                                               // DO NOT TRANSLATE
             /*10 | 10 | 2006-Jan-15*/         new Regex(@"\b" + YearEx + @"[-/\., ]*" + MonthStrEx + @"[-/\., ]*" + DayEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                     // DO NOT TRANSLATE
             /*11 | 11 | 22 Dec 2000/2001*/    new Regex(@"\b" + DayEx + @"[-/\., ]*" + MonthStrEx + @"[-/\., ]*" + DoubleYearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                               // DO NOT TRANSLATE
             /*12 | 12 | 22 Dec 2000-2001*/    new Regex(@"\b" + DayEx + @"[-/\., ]*" + MonthStrEx + @"[-/\., ]*" + YearEx + @"[-]([0-9]+)" + BcAdEx + @"\b", RegexOptions.Compiled),                                    // DO NOT TRANSLATE
             /*13 | 13 | 22 Dec 2000*/         new Regex(@"\b" + DayEx + @"[-/\., ]*" + MonthStrEx + @"[-/\., ]*" + YearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                     // DO NOT TRANSLATE
             /*14 | 14 | 22 Dec 20/21*/        new Regex(@"\b" + DayEx + @"[-/\., ]*" + MonthStrEx + @"[-/\., ]*" + TwoDigitDoubleYearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                       // DO NOT TRANSLATE
             /*15 | 15 | 22 Dec 2/3*/          new Regex(@"\b" + DayEx + @"[-/\., ]*" + MonthStrEx + @"[-/\., ]*" + OneDigitDoubleYearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                       // DO NOT TRANSLATE
             /*16 | 16 | 22 Dec 20*/           new Regex(@"\b" + DayEx + @"[-/\., ]*" + MonthStrEx + @"[-/\., ]*" + TwoYearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                  // DO NOT TRANSLATE
             /*17 | 17 | Dec 22, 2000/2001*/   new Regex(@"\b" + MonthStrEx + @"[-/\., ]*" + DayEx + @"[-/\., ]+" + DoubleYearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                               // DO NOT TRANSLATE
             /*18 | 18 | Dec 22, 2000-2001*/   new Regex(@"\b" + MonthStrEx + @"[-/\., ]*" + DayEx + @"[-/\., ]+" + YearEx + @"[-]([0-9]+)" + BcAdEx + @"\b", RegexOptions.Compiled),                                    // DO NOT TRANSLATE
             /*19 | 19 | Dec 22, 2000*/        new Regex(@"\b" + MonthStrEx + @"[-/\., ]*" + DayEx + @"[-/\., ]+" + YearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                     // DO NOT TRANSLATE
             /*20 | 20 | Dec 22, 20*/          new Regex(@"\b" + MonthStrEx + @"[-/\., ]*" + DayEx + @"[-/\., ]+" + TwoYearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                  // DO NOT TRANSLATE
             /*21 | 21 | Dec [Qtr] 2000/2001*/ new Regex(@"\b" + MonthStrEx + @"[-/\., ]*" + Quarter + @"[-/\., ]*" + DoubleYearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                             // DO NOT TRANSLATE
             /*22 | 21a | Dec 2000/2001*/      new Regex(@"\b" + MonthStrEx + @"[-/\., ]*" + DoubleYearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                                      // DO NOT TRANSLATE
             /*23 | 22 | Dec [Qtr] 2000-2001*/ new Regex(@"\b" + MonthStrEx + @"[-/\., ]*" + Quarter + @"[-/\., ]*" + YearEx + @"[-]([0-9]+)" + BcAdEx + @"\b", RegexOptions.Compiled),                                  // DO NOT TRANSLATE                
             /*24 | 22a | Dec 2000-2001*/      new Regex(@"\b" + MonthStrEx + @"[-/\., ]*" + YearEx + @"[-]([0-9]+)" + BcAdEx + @"\b", RegexOptions.Compiled),                                                           // DO NOT TRANSLATE
             /*25 | 23 | Dec [Qtr] 2000*/      new Regex(@"\b" + MonthStrEx + @"[-/\., ]*" + Quarter + @"[-/\., ]*" + YearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                   // DO NOT TRANSLATE
             /*26 | 23a | Dec 2000*/           new Regex(@"\b" + MonthStrEx + @"[-/\., ]*" + YearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                                            // DO NOT TRANSLATE
             /*27 | 24 | Dec [Qtr] 40/41*/     new Regex(@"\b" + MonthStrEx + @"[-/\., ]*" + Quarter + @"[-/\., ]*" + "(3[2-9]|[4-9][0-9])/([0-9]+)" + BcAdEx + @"\b", RegexOptions.Compiled),                           // DO NOT TRANSLATE         
             /*28 | 24a | Dec 40/41*/          new Regex(@"\b" + MonthStrEx + @"[-/\., ]*(3[2-9]|[4-9][0-9])/([0-9]+)" + BcAdEx + @"\b", RegexOptions.Compiled),                                                         // DO NOT TRANSLATE
             /*29 | 25 | Dec [Qtr] 40*/        new Regex(@"\b" + MonthStrEx + @"[-/\., ]*" + Quarter + @"[-/\., ]*" + "(3[2-9]|[4-9][0-9])" + BcAdEx + @"\b", RegexOptions.Compiled),                                    // DO NOT TRANSLATE             
             /*30 | 25a | Dec 40*/             new Regex(@"\b" + MonthStrEx + @"[-/\., ]*(3[2-9]|[4-9][0-9])" + BcAdEx + @"\b", RegexOptions.Compiled),                                                                  // DO NOT TRANSLATE
             /*31 | 25b | Dec 4 AD*/           new Regex(@"\b" + MonthStrEx + @"[-/\., ]*([1-9]|[0-9][0-9]) *(" + BceRegex + "|" + BcRegex + "|" + AdRegex + "|" + CeRegex + @")\b", RegexOptions.Compiled),             // DO NOT TRANSLATE
             /*32 | 26 | Dec 22*/              new Regex(@"\b" + MonthStrEx + @"[-/\., ]*" + DayEx + @" *\b", RegexOptions.Compiled),                                                                                    // DO NOT TRANSLATE
             /*33 | 27 | 12 2000/01*/          new Regex(@"\b" + MonthEx + @"[- \.,]+" + DoubleYearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                                          // DO NOT TRANSLATE 
             /*34 | 28 | 2000/01*/             new Regex(@"\b" + DoubleYearEx + BcAdEx, RegexOptions.Compiled),                                                                                                          // DO NOT TRANSLATE
             /*35 | 29 | 12/2000*/             new Regex(@"\b" + MonthEx + @"[- \./,]+" + AnyYearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                                            // DO NOT TRANSLATE
             /*36 | 30 | 12/2000*/             new Regex(@"\b" + MonthEx + @"[- \./,]+" + YearEx + BcAdEx + @"\b", RegexOptions.Compiled),                                                                               // DO NOT TRANSLATE
             /*37 | 31 | 2006 bc/ad*/          new Regex(@"\b" + YearEx + @"[-/\., ]*" + BcAdEx + @"\b", RegexOptions.Compiled),                                                                                         // DO NOT TRANSLATE
             /*38 | 32 | 2006-Jan*/            new Regex(@"\b" + YearEx + @"[-/\., ]*" + MonthStrEx + @"\b", RegexOptions.Compiled),                                                                                     // DO NOT TRANSLATE
             /*39 | 33 | 2000*/                new Regex(@"\b" + YearEx + BcAdEx, RegexOptions.Compiled),                                                                                                                // DO NOT TRANSLATE   
             /*40 | 34 | 20 (year) bc/ad*/     new Regex(@"\b" + TwoYearEx + @" *(" + BceRegex + "|" + BcRegex + "|" + AdRegex + "|" + CeRegex + ")", RegexOptions.Compiled),                                            // DO NOT TRANSLATE
             /*41 | 35 | 22 Dec*/              new Regex(@"\b" + DayEx + @"[-/\., ]*" + MonthStrEx + @"\b", RegexOptions.Compiled),                                                                                      // DO NOT TRANSLATE
             /*42 | 36 | 20 (year)*/           new Regex(@"\b" + TwoYearEx, RegexOptions.Compiled),                                                                                                                      // DO NOT TRANSLATE
             /*43 | 37 | Jan*/                 new Regex(@"\b" + MonthStrEx + @"\b",  RegexOptions.Compiled),                                                                                                            // DO NOT TRANSLATE
             /*44 | parser01 | */              new Regex(string.Format("(?i)(-)|(?i)(/)|(\u2013)|{0}|{1}", CreateRegExFromResource(DateParserStrings.ResourceManager.GetString("Between", _culture), true), CreateRegExFromResource(DateParserStrings.ResourceManager.GetString("Range", _culture), true)), RegexOptions.Compiled),
             /*45 | illegal1 | */              new Regex(@"^ *([0-9]+)[ ./\-,]+([0-9][0-9][0-9]+)[ ./\-,]*\p{L}+(?i)(?<!\b(" + BceRegex + "|" + BcRegex + "|" + AdRegex + "|" + CeRegex + ")) *$", RegexOptions.Compiled),
             /*46 | illegal2 | */              new Regex(@"^ *[0-9]+[ ./\-,]+([0-9][0-9][0-9]+)[ ./\-,]+[0-9]+ *$", RegexOptions.Compiled),
             /*47 | range1 | */                new Regex(string.Format("^ *({0}|-|{{~}}|\\u2013) *$", DateParserStrings.ResourceManager.GetString("To", _culture)), RegexOptions.Compiled),
             /*48 | range2 | */                new Regex(string.Format(@" *({0}|{1}|{{~}}|-|\\u2013|\.) *", DateParserStrings.ResourceManager.GetString("To", _culture), DateParserStrings.ResourceManager.GetString("And", _culture)), RegexOptions.Compiled),
            };
        }

        public static bool DayBeforeMonth;// default value is false
        public static int DoubleDateCutoffYear=1753;
        public static bool? TwoDigitYearAsCurrentCentury=null;
        public static bool? AutoCreateDoubleDates = null;
        public static bool AncientDateTypeWarning = true;
        private static bool _commonEra = false;
        public static bool CommonEra
        {
            get { return _commonEra; }
            set { _commonEra = value; }
        }

        protected static uint today;
        Dictionary<string, DateModifier> WordsToQualifier;

        // The Months array allows the program to interpret both local month names and English month names, regardless of language settings.
        string[,] Months;
        static int[] MonthMaxDays = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        static int _MAX_LDS_KEYWORDS = 4;
        //TODO: These strings should come from Resources, we need to do this differently so we don't run out of room for multiple strings
        static string[,] _LdsKeywords = {
            {Properties.Resources.DateKeywordBIC, "bic", null, null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordCancelled, "cancelled", "can", "canceled"}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordChild, "child", "chi", "infant"}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordCleared, "cleared", "cle", null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordCompleted, "completed", "com", null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordDead, "dead", null, null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordDeceased, "deceased", null, null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordDone,"done", null, null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordDNS, "dns", null, null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordDNSCAN, "dns/can", null, null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordInfant, "infant", "inf", null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordNeverMarried, "never married",null, null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordPre1970, "pre-1970", null, null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordStillborn, "stillborn", "sti", null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordSubmitted, "submitted", "sub", null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordUncleared, "uncleared", "unc", null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordYoung, "young", null, null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordUnknown, "unknown", "?", "unk"}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordPrivate, "private", null, null}, // DO NOT TRANSLATE
            {Properties.Resources.DateKeywordNotMarried, "not married",null, null} // DO NOT TRANSLATE
        };

        public static string LdsKeyword(int i)
        {
            return _LdsKeywords[i,0];
        }

        DateModifier _Qualifier;

        static readonly string DayEx;
        static readonly string MonthEx;
        static readonly string YearEx;
        static readonly string DoubleYearEx;
        static readonly string TwoDigitDoubleYearEx;
        static readonly string OneDigitDoubleYearEx;
        static readonly string TwoYearEx;
        static readonly string AnyYearEx;
        static readonly string MonthStrEx;
        string BcRegex;
        string BceRegex;
        string AdRegex;
        string CeRegex;
        string BcAdEx;
        string Quarter;

        static readonly int[,] parseIndexes =
            {
              // D,M,Y,Y2,numeric, Qtr
                {2,1,3,0,1,0}, //(1)
                {2,1,3,4,1,0}, //(2)
                {2,1,3,0,1,0}, //(3)
                {2,1,3,0,0,0}, //(4)
                {2,1,3,0,0,0}, //(5)
                {2,1,3,0,1,0}, //(6)
                {2,1,3,4,1,0}, //(7)
                {2,1,3,0,1,0}, //(8)
                {3,2,1,0,1,0}, //(9)
                {0,2,1,0,1,0}, //(9a)
                {3,2,1,0,0,0}, //(10)
                {1,2,3,0,0,0}, //(11)
                {1,2,3,4,0,0}, //(12)
                {1,2,3,0,0,0}, //(13)
                {1,2,3,0,0,0}, //(14)
                {1,2,3,0,0,0}, //(15)
                {1,2,3,0,0,0}, //(16)
                {2,1,3,0,0,0}, //(17)
                {2,1,3,4,0,0}, //(18)
                {2,1,3,0,0,0}, //(19)
                {2,1,3,0,0,0}, //(20)
                {0,1,3,4,0,2}, //(21)
                {0,1,2,0,0,0}, //(21a)
                {0,1,3,4,0,2}, //(22)
                {0,1,2,3,0,0}, //(22a)
                {0,1,3,0,0,2}, //(23)
                {0,1,2,0,0,0}, //(23a)
                {0,1,3,4,0,2}, //(24)
                {0,1,2,3,0,0}, //(24a)
                {0,1,3,0,0,2}, //(25)
                {0,1,2,0,0,0}, //(25a)
                {0,1,2,0,0,0}, //(25b)
                {2,1,0,0,0,0}, //(26)
                {0,1,2,0,1,0}, //(27)
                {0,0,1,0,0,0}, //(28)
                {0,1,2,0,1,0}, //(29)
                {0,1,2,0,1,0}, //(30)
                {0,0,1,0,0,0}, //(31)
                {0,2,1,0,0,0}, //(32)
                {0,0,1,0,0,0}, //(33)
                {0,0,1,0,0,0}, //(34)
                {1,2,0,0,0,0}, //(35)
                {0,0,1,0,0,0}, //(36)
                {0,1,0,0,0,0}  //(37)
            };

        uint? _dateFirst;
        uint? _dateSecond;
        string _orgEntry;

        private static string CreateRegExFromResource(string rsrc)
        {
            return CreateRegExFromResource(rsrc, false);
        }

        private static string CreateRegExFromResource(string rsrc, bool groupTerms)
        {
            string[] words = rsrc.Split(',');
            StringBuilder sb = new StringBuilder();

            foreach (string word in words)
            {
                if (sb.Length > 0)
                {
                    sb.Append("|");
                }
                if (groupTerms) sb.Append('(');
                sb.Append(Regex.Escape(word));
                if (groupTerms) sb.Append(')');
            }

            return sb.ToString();
        }

        private static string CreatePunctuatedRegExFromResource(string rsrc, bool groupTerms)
        {
            string[] words = rsrc.Split(',');
            StringBuilder sb = new StringBuilder();

            foreach (string word in words)
            {
                if (sb.Length > 0)
                {
                    sb.Append("|");
                }
                if (groupTerms) sb.Append('(');
                sb.Append(Regex.Escape(word).Replace("\\.", "\\.?"));
                if (groupTerms) sb.Append(')');
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets or sets the first date.
        /// </summary>
        /// <value>The first date.</value>
        public uint? FirstDate
        {
            get { return _dateFirst; }
            set { _dateFirst = value; }
        }

        /// <summary>
        /// Gets or sets the second date.
        /// </summary>
        /// <value>The second date.</value>
        public uint? SecondDate
        {
            get { return _dateSecond; }
            set { _dateSecond = value; }
        }

        public DateParseErrorType Reason
        {
            get { return _dateParseErrorInfo[0].Reason; }
        }

        public string Data
        {
            get { return _dateParseErrorInfo[0].Data; }
        }

        IList<DateParseErrorInfo> _dateParseErrorInfo = null;


        public bool ParseDate(string dateStr)
        {
            _orgEntry = dateStr;
            bool retVal = Parse(dateStr, out _dateFirst, out _dateSecond, out _Qualifier);
            return retVal;
        }

        private static uint? StripQualifiers(uint? date)
        {
            if (!date.HasValue) return date;

            return (date.Value) & ~(uint)(DateModifier.About | DateModifier.After | DateModifier.Before | DateModifier.Calculated);
        }

        private static bool IsADate(uint? date)
        {
            return date != null && (date & 0x7FFFFE00) > 0;
        }


        static long ticks = 0;
        static long outsideTicks = 0;
        static long outsideTicksStart = 0;
        static int calls = 0;
        static TimeSpan[] OutsideTimespan = new TimeSpan[20];
        static TimeSpan[] InsideTimespan = new TimeSpan[20];
        static int currentYear = DateTime.Now.Year;
        static bool _ignoreDoubleDates;
        /// <summary>
        /// Parses the specified date string.
        /// </summary>
        /// <param name="dateStr">The date string.</param>
        [System.Diagnostics.DebuggerStepThrough] // this is so we don't have to stop on all the possible exceptions
        public bool Parse(string dateStr, out uint? firstDate, out uint? secondDate, out DateModifier qualifier)
        {
            // special case import format
            if (dateStr.IndexOf("-") > -1 && dateStr.IndexOf("0/") > -1) // reduce the # of times this regex is called // DO NOT TRANSLATE
            {
                Match specialMatch = s_parsers["special0"].Match(dateStr); // DO NOT TRANSLATE
                if (specialMatch.Groups.Count > 1 && dateStr.Equals(specialMatch.Groups[0].Value))
                {
                    dateStr = "Bet. " + specialMatch.Groups[1] + "/" + specialMatch.Groups[2] + "/" + specialMatch.Groups[3] + " - " + specialMatch.Groups[1] + "/" + specialMatch.Groups[4] + "/" + specialMatch.Groups[3]; // DO NOT TRANSLATE
                }
            }
            _ignoreDoubleDates = false;
            if (dateStr.IndexOf(Properties.Resources.ResourceManager.GetString("DateBC", _culture) + ":") == 0) // DO NOT TRANSLATE
            {
                dateStr = dateStr.Substring(3) + " " + Properties.Resources.ResourceManager.GetString("DateBC", _culture).ToLower(); // DO NOT TRANSLATE
            }
            if (outsideTicksStart != 0)
                outsideTicks += DateTime.Now.Ticks - outsideTicksStart;
            ++calls;
            long startTicks = DateTime.Now.Ticks;
            firstDate = null;
            secondDate = null;
            qualifier = DateModifier.None;

            bool retVal = false;
            bool aboutBraces = false;
            dateStr = dateStr.Trim();
            if (dateStr[0] == '<' && dateStr[dateStr.Length - 1] == '>')
            {
                dateStr = dateStr.Substring(1, dateStr.Length - 2);
                aboutBraces = true;
            }

            // Quick check for single date in standard form "dd MMM yyyy"
            Match match = s_parsers["standard"].Match(dateStr); // DO NOT TRANSLATE
            if (match.Success && match.Value.Equals(dateStr))
            {
                int day = Convert.ToInt32(match.Groups[1].Value);
                int year = Convert.ToInt32(match.Groups[3].Value);
                if (day < 32 && year > DoubleDateCutoffYear && year < currentYear)
                {
                    int month = GetMonth(match.Groups[2].Value);
                    if (month > 0
                        && !(month == 2 && day > 27)  // invalid month or February (don't want to deal with leap years)
                        && day <= MonthMaxDays[month - 1] // correct number of days
                        && day > 0) // must be positive
                    {
                        firstDate = SDNDate.Encode((int?)year, (int?)month, (int?)day, DateModifier.None);
                        retVal = true;


                        ticks += DateTime.Now.Ticks - startTicks;
                        if (calls % 5000 == 0)
                        {
                            int pos = calls / 5000;
                            OutsideTimespan[pos] = new TimeSpan(outsideTicks);
                            InsideTimespan[pos] = new TimeSpan(ticks);
                        }
                        outsideTicksStart = DateTime.Now.Ticks;
                    }
                }
            }

            if (!retVal)
            {
                // SPECIAL CASE
                try
                {
                    match = s_parsers["special1"].Match(dateStr); // DO NOT TRANSLATE
                    if (match.Groups.Count == 4)
                    {
                        dateStr = match.Groups[1].Value + " " + match.Groups[3].Value + " - " // DO NOT TRANSLATE
                            + match.Groups[2].Value + " " + match.Groups[3].Value; // DO NOT TRANSLATE
                    }
                }
                catch
                {
                }
                try
                {
                    match = s_parsers["special2"].Match(dateStr); // DO NOT TRANSLATE
                    if (match.Groups.Count == 5)
                    {
                        if (!match.Groups[2].Value.Equals("00") && !match.Groups[3].Value.Equals("00") && match.Groups[1].Value.Equals(match.Groups[4].Value)) // DO NOT TRANSLATE
                            dateStr = match.Groups[2].Value + "/" + match.Groups[3].Value + "/" + match.Groups[1] + "/" + (Convert.ToInt32(match.Groups[4].Value) + 1); // DO NOT TRANSLATE
                    }
                }
                catch
                {
                }
                try
                {
                    if (dateStr.IndexOf(Properties.Resources.ResourceManager.GetString("DateOr", _culture)) == 0) // DO NOT TRANSLATE
                    {
                        dateStr = Properties.Resources.ResourceManager.GetString("DateBetween", _culture) + dateStr.Substring(Properties.Resources.ResourceManager.GetString("DateOr", _culture).Length); // DO NOT TRANSLATE
                        _ignoreDoubleDates = true;                        
                    }
                    if (dateStr.IndexOf(Properties.Resources.ResourceManager.GetString("DateAbout", _culture)) == 0 && // DO NOT TRANSLATE
                        dateStr.IndexOf("-") != -1 && // DO NOT TRANSLATE
                        dateStr.IndexOf("-") == dateStr.LastIndexOf("-")) // DO NOT TRANSLATE
                    {
                        dateStr = Properties.Resources.ResourceManager.GetString("DateBetween", _culture) + dateStr.Substring(Properties.Resources.ResourceManager.GetString("DateAbout", _culture).Length); // DO NOT TRANSLATE
                    }
                    if (dateStr.IndexOf(Properties.Resources.ResourceManager.GetString("DateBetween", _culture)) == 0 ||  // DO NOT TRANSLATE
                        dateStr.IndexOf("EstBetween:") == 0 || // DO NOT TRANSLATE
                        dateStr.IndexOf(Properties.Resources.ResourceManager.GetString("DateRange", _culture)) == 0 || // DO NOT TRANSLATE
                        dateStr.IndexOf("Bet") == 0) // DO NOT TRANSLATE
                    {
                        dateStr = dateStr.Replace("-", " and "); // special case for genbridge // DO NOT TRANSLATE
                    }
                    ParseData parseData = new ParseData(dateStr, firstDate, secondDate);
                    parseData.DateStr = parseData.DateStr.Replace("\u2013", "-"); // DO NOT TRANSLATE
                    Match rangeMatch = _parsers[PARSER01].Match(parseData.DateStr); // DO NOT TRANSLATE
                    bool mayBeRange = rangeMatch.Groups.Count > 1;

                    string orgDateString = parseData.DateStr;
                    retVal = Parse(parseData);
                    if (mayBeRange && parseData.SecondDate != null)
                    {
                        int firstIndex = orgDateString.IndexOf("-"); // DO NOT TRANSLATE
                        if (!(firstIndex > -1 && orgDateString.LastIndexOf("-") != firstIndex) || // DO NOT TRANSLATE
                            (orgDateString.IndexOf(DateParserStrings.ResourceManager.GetString("And", _culture)) > 0 || orgDateString.IndexOf(DateParserStrings.ResourceManager.GetString("Or", _culture)) > 0)) // DO NOT TRANSLATE
                        {
                            parseData.DateParseErrorInfo.Clear(); // reset errors
                            parseData.FirstDate = null;
                            parseData.SecondDate = null;
                            parseData.DateStr = orgDateString;
                            string originalStr = parseData.DateStr;
                            string dateStrCopy = originalStr;
                            dateStrCopy = Regex.Replace(dateStrCopy, string.Format(@"(?i)\b({0}|{1}|-|\u2013)\b",
                                DateParserStrings.ResourceManager.GetString("And", _culture),
                                DateParserStrings.ResourceManager.GetString("To", _culture)), " {~} "); // DO NOT TRANSLATE
                            dateStrCopy = Regex.Replace(dateStrCopy, string.Format(@"(?i)\b({0}|{1}|{2}|{3})", 
                                CreateRegExFromResource(DateParserStrings.ResourceManager.GetString("Between", _culture)), 
                                CreateRegExFromResource(DateParserStrings.ResourceManager.GetString("Before", _culture)),
                                CreateRegExFromResource(DateParserStrings.ResourceManager.GetString("After", _culture)),
                                CreateRegExFromResource(DateParserStrings.ResourceManager.GetString("Range", _culture))), ""); // DO NOT TRANSLATE
                            dateStrCopy = dateStrCopy.Replace(":", ""); // DO NOT TRANSLATE
                            dateStrCopy = dateStrCopy.Replace("-", "{~}"); // DO NOT TRANSLATE
                            //dateStrCopy = dateStrCopy.Replace("/", "{~}"); // DO NOT TRANSLATE
                            string firstStr = dateStrCopy.Substring(0, dateStrCopy.IndexOf("{~}")).Trim(); // DO NOT TRANSLATE
                            string secondStr = dateStrCopy.Substring(dateStrCopy.IndexOf("{~}") + 3).Trim(); // DO NOT TRANSLATE
                            uint? firstDt = null, secondDt = null;
                            Match firstMatch = Regex.Match(firstStr, "([0-3][0-9]|[0-9])"); // DO NOT TRANSLATE
                            if ((firstMatch.Groups.Count == 1 || firstMatch.Groups.Count == 2) && firstMatch.Groups[0].Value.Equals(firstStr) && Convert.ToInt32(firstStr) < 32)
                            {
                                parseData.DateStr = secondStr;
                                retVal = Parse(parseData);
                                secondDt = parseData.FirstDate;
                                int? year1, month1;
                                DateModifier modifier1;
                                SDNDate sdnDate = new SDNDate((uint)secondDt);
                                year1 = sdnDate.Year;
                                month1 = sdnDate.Month;
                                modifier1 = sdnDate.Modifier;
                                firstDt = SDNDate.Encode(year1, month1, (int?)Convert.ToInt32(firstStr), modifier1);
                            }
                            else
                            {
                                parseData.DateStr = firstStr;
                                retVal = Parse(parseData);
                                firstDt = parseData.FirstDate;

                                parseData.ParsingSecondDate = true;
                                parseData.DateStr = secondStr;
                                retVal = Parse(parseData);
                                secondDt = parseData.FirstDate;
                            }
                            CopyYears(ref firstDt, ref secondDt);
                            parseData.ParsingSecondDate = false;
                            if (firstDt > secondDt)
                            {
                                CorrectDateRangeOrder(ref firstDt, ref secondDt);
                                parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.RangeError, originalStr, true));
                            }
                            parseData.FirstDate = firstDt;
                            parseData.SecondDate = secondDt;
                        }
                    }
                    firstDate = parseData.FirstDate;
                    secondDate = parseData.SecondDate;
                    qualifier = parseData.Qualifier;

                    // if there are two dates, strip out all qualifiers
                    if (secondDate != null && secondDate != 0)
                    {
                        firstDate = StripQualifiers(firstDate);
                        secondDate = StripQualifiers(secondDate);
                    }

                    if (today == 0)
                        today = SDNDate.Encode(DateTime.Now);

                    if ((firstDate & 0x7FFFFE00) > today)
                    {
                        parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.FutureDate, dateStr, parseData.ParsingSecondDate));
                    }
                    if (dateStr.Length > 0)
                        dateStr = dateStr.Trim();
                    if (IsADate(parseData.FirstDate) && IsADate(parseData.SecondDate) && !mayBeRange)
                    {
                        parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.UnknownContent, dateStr, true));
                    }
                    if (parseData.DateParseErrorInfo.Count > 0)
                    {
                        SetErrorsInPriorityOrder(parseData);
                        throw new DateParseException(new DateParseError(parseData.DateParseErrorInfo, parseData.FirstDate, parseData.SecondDate));
                    }
                }
                finally
                {
                    ticks += DateTime.Now.Ticks - startTicks;
                    if (calls % 5000 == 0)
                    {
                        int pos = calls / 5000;
                        OutsideTimespan[pos] = new TimeSpan(outsideTicks);
                        InsideTimespan[pos] = new TimeSpan(ticks);
                    }
                    outsideTicksStart = DateTime.Now.Ticks;
                }
                if (aboutBraces)
                {
                    firstDate |= (uint)DateModifier.About;
                }
            }

            return retVal;
        }


        static uint? ParseLdsKeywords(string dateStr)
        {
            uint? retVal = null;
            string date = (dateStr.Substring(0, 1).Equals("?") && dateStr.Length > 1) ? dateStr.Substring(1) : dateStr; // ignore initial question mark // DO NOT TRANSLATE
            for (int i = 0; i < _LdsKeywords.Length / _MAX_LDS_KEYWORDS; i++)
            {
                for (int n = 1; n < _MAX_LDS_KEYWORDS && _LdsKeywords[i, n] != null; n++)
                {
                    if (date.ToLower().Equals(_LdsKeywords[i, n].ToLower()))
                    {
                        retVal = (uint)0x80000000 + (uint)i;
                        break;
                    }
                }
            }
            return retVal;
        }

        protected void FindIllegalPatterns(ParseData parseData)
        {
            //Match match = Regex.Match(parseData.DateStr, "^ *([0-9][0-9][0-9]+)[ ,./]+([0-9][0-9][0-9]+) *$"); // DO NOT TRANSLATE
            //if (match.Groups.Count > 1)
            //{
            //    int year = Convert.ToInt32(match.Groups[2].Value);
            //    parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.MonthOutOfRange, match.Groups[1].Value, parseData.FirstDate != null || parseData.ParsingSecondDate));
            //    throw new DateParseException(new DateParseError(parseData.DateParseErrorInfo, Date.Encode(year, 0, 0, DateModifier.DayMissing | DateModifier.MonthMissing),0));
            //}
            Match match = _parsers[ILLEGAL1].Match(parseData.DateStr); // DO NOT TRANSLATE
            if (match.Groups.Count > 1)
            {
                int year = Convert.ToInt32(match.Groups[2].Value);
                parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.UnknownContent, parseData.DateStr, parseData.FirstDate != null || parseData.ParsingSecondDate));
                throw new DateParseException(new DateParseError(parseData.DateParseErrorInfo, SDNDate.Encode(year, 0, 0, DateModifier.DayMissing | DateModifier.MonthMissing), 0));
            }
            match = _parsers[ILLEGAL2].Match(parseData.DateStr); // Don't accept "12 1999 3" // DO NOT TRANSLATE
            if (match.Groups.Count > 1)
            {
                int year = Convert.ToInt32(match.Groups[1].Value);
                int slashIndex = parseData.DateStr.IndexOf("/"); // DO NOT TRANSLATE
                if (slashIndex == -1 || (slashIndex != parseData.DateStr.LastIndexOf("/"))) //(No error for  "3 1622/23") // DO NOT TRANSLATE
                {
                    parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.UnknownContent, parseData.DateStr, parseData.FirstDate != null || parseData.ParsingSecondDate));
                    throw new DateParseException(new DateParseError(parseData.DateParseErrorInfo, SDNDate.Encode(year, 0, 0, DateModifier.DayMissing | DateModifier.MonthMissing), 0));
                }
            }
        }


        protected bool Parse(ParseData parseData)//string dateStr, out uint? firstDate, out uint? secondDate, out DateModifier Qualifier)
        {
            bool retVal = false;
            FindIllegalPatterns(parseData);
            string dateStr = parseData.DateStr;
            string origDateStr = dateStr;
            InitializeWordsToQualifier();
            dateStr = dateStr.Trim().ToLower();
            parseData.Qualifier = DateModifier.None;
            //parseData.FirstDate = parseData.SecondDate = 0;
            if ((parseData.FirstDate = ParseLdsKeywords(dateStr)) > 0)
            {
                retVal = true;
            }
            else
            {
                foreach (string key in WordsToQualifier.Keys)
                {
                    if (key != null && dateStr.IndexOf(key) == 0)
                    {
                        string str = key + "\\.?"; // DO NOT TRANSLATE
                        Match match = Regex.Match(dateStr, "(^" + str + ")[0-9 :]"); // DO NOT TRANSLATE
                        if (match.Groups.Count > 1 && 
                            match.Groups[1].Value.Equals(key) ||
                            match.Groups[1].Value.Equals(key + ".")) // DO NOT TRANSLATE
                        {
                            parseData.Qualifier = WordsToQualifier[key];
                            dateStr = dateStr.Substring(match.Groups[1].Value.Length).Trim();
                            break;
                        }
                    }
                }
                dateStr = Regex.Replace(dateStr, string.Format(@"(?i)\b({0}|{1})\b",
                    DateParserStrings.ResourceManager.GetString("And", _culture),
                    DateParserStrings.ResourceManager.GetString("To", _culture)), " {~} "); // DO NOT TRANSLATE
                dateStr = Regex.Replace(dateStr, string.Format(@"(?i)\b({0}|{1}|{2}|{3})",
                    CreateRegExFromResource(DateParserStrings.ResourceManager.GetString("Between", _culture)),
                    CreateRegExFromResource(DateParserStrings.ResourceManager.GetString("Range", _culture)),
                    CreateRegExFromResource(DateParserStrings.ResourceManager.GetString("Before", _culture)),
                    CreateRegExFromResource(DateParserStrings.ResourceManager.GetString("After", _culture)))
                    , ""); // DO NOT TRANSLATE
                dateStr = dateStr.Replace(":", ""); // DO NOT TRANSLATE

                for (int i = 0; i < NUMPARSERS; i++)
                {
                    dateStr = ParseDate(dateStr, _parsers[i], parseIndexes[i, 0], parseIndexes[i, 1], parseIndexes[i, 2], parseIndexes[i, 3], parseIndexes[i, 4] != 0, parseIndexes[i,5], ref parseData);
                    if (dateStr.Length == 0)
                    {
                        retVal = true;
                        break;
                    }
                    //if (parseData.DateParseErrorInfo.Count > 0)
                    //{
                    //    retVal = false;
                    //    return retVal;
                    //}
                    if (parseData.FirstDate != null && parseData.FirstDate != 0 &&
                        parseData.SecondDate != null && parseData.SecondDate != 0)
                    {
                        Match match = _parsers[RANGE1].Match(dateStr); // DO NOT TRANSLATE
                        if (!(match.Groups.Count > 0 && match.Groups[0].Value.Equals(dateStr)))
                        {
                            parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.UnknownContent, parseData.DateStr, parseData.FirstDate != null || parseData.ParsingSecondDate));
                            retVal = false;
                            return retVal;
                        }
                    }
                }
                uint? firstDate = parseData.FirstDate;
                uint? secondDate = parseData.SecondDate;
                CopyYears(ref firstDate, ref secondDate);
                //CorrectDateRangeOrder(ref firstDate, ref secondDate);
                if (firstDate > secondDate)
                {
                    CorrectDateRangeOrder(ref firstDate, ref secondDate);
                    //parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.RangeError, origDateStr));
                }
                parseData.FirstDate = firstDate == 0 ? null : firstDate;
                parseData.SecondDate = secondDate == 0 ? null : secondDate;
                //TODO: these strings need to be localizable
                if (dateStr.Length > 0)
                {
                    Match match = _parsers[RANGE2].Match(dateStr); // DO NOT TRANSLATE
                    if (match.Groups.Count == 0 || !match.Groups[0].Value.Equals(dateStr))
                    {
                        //throw new DateParseException(new DateParseError(DateParseErrorType.UnknownContent, dateStr));
                        parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.UnknownContent, dateStr, parseData.FirstDate != null || parseData.ParsingSecondDate));
                    }
                }
            }
            return retVal;
        }

        protected static void CopyYears(ref uint? firstDate, ref uint? secondDate)
        {
            if (firstDate != null && firstDate != 0 &&
                secondDate != null && secondDate != 0)
            {
                Date dt1 = Date.CreateInstance((uint)firstDate);
                Date dt2 = Date.CreateInstance((uint)secondDate);
                int? year1, month1, day1;
                int? year2, month2, day2;
                DateModifier modifier1, modifier2;
                year1 = dt1.Year;
                month1 = dt1.Month;
                day1 = dt1.Day;
                modifier1 = dt1.Modifier;
                year2 = dt2.Year;
                month2 = dt2.Month;
                day2 = dt2.Day;
                modifier2 = dt2.Modifier;

                bool encode1 = false;
                bool encode2 = false;
                if ((year1 == null || year1 == 0) && (year2 != null && year2 != 0))
                {
                    year1 = year2;
                    modifier1 &= ~DateModifier.YearMissing;
                    encode1 = true;
                }
                if ((year2 == null || year2 == 0) && (year1 != null && year1 != 0))
                {
                    year2 = year1;
                    modifier2 &= ~DateModifier.YearMissing;
                    encode2 = true;
                }
                if (encode1)
                    firstDate = SDNDate.Encode(year1, month1, day1, modifier1);
                if (encode2)
                    secondDate = SDNDate.Encode(year2, month2, day2, modifier2);
            }
        }

        protected static void CorrectDateRangeOrder(ref uint? firstDate, ref uint? secondDate)
        {
            if (firstDate > secondDate && secondDate != 0)
            {
                uint? date = firstDate;
                firstDate = secondDate;
                secondDate = date;
            }
        }

        protected int GetDateMonth(Match match, int monthIndex, bool monthIsNumber, ref ParseData parseData)
        {
            int month = 0;
            if (monthIndex > 0)
            {
                if (monthIsNumber)
                {
                    month = Convert.ToInt32(match.Groups[monthIndex].Value);
                }
                else
                {
                    month = GetMonth(match.Groups[monthIndex].Value);
                    if (month == 0)
                    {
                        //throw new DateParseException(new DateParseError(DateParseErrorType.UnknownContent, match.Groups[monthIndex].Value));
                        parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.UnknownContent, match.Groups[monthIndex].Value, parseData.FirstDate != null || parseData.ParsingSecondDate));
                    }
                }
            }
            return month;
        }

        protected static int GetDateDay(Match match, int dayIndex)
        {
            if (dayIndex > 0)
            {
                return Convert.ToInt32(match.Groups[dayIndex].Value);
            }
            return 0;
        }

        protected bool IsBC(ref ParseData parseData, string bcad)
        {
            bool isBc = false;
            if (bcad != null && bcad.Length > 0)
            {
                Match bcMatch = Regex.Match(bcad, BcRegex);
                Match bceMatch = Regex.Match(bcad, BceRegex);
                Match adMatch = Regex.Match(bcad, AdRegex);
                Match ceMatch = Regex.Match(bcad, CeRegex);
                if (bceMatch.Groups.Count == 1 && bceMatch.Groups[0].Value.Equals(bcad))
                {
                    isBc = true;
                    if (AncientDateTypeWarning && !_commonEra)
                        parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.WrongAncientDateFormat, bcad, parseData.FirstDate != null || parseData.ParsingSecondDate));
                }
                else if (bcMatch.Groups.Count == 1 && bcMatch.Groups[0].Value.Equals(bcad))
                {
                    isBc = true;
                    if (AncientDateTypeWarning && _commonEra)
                        parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.WrongAncientDateFormat, bcad, parseData.FirstDate != null || parseData.ParsingSecondDate));
                }
                if (adMatch.Groups.Count == 1 && adMatch.Groups[0].Value.Equals(bcad))
                {
                    if (AncientDateTypeWarning && _commonEra)
                        parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.WrongAncientDateFormat, bcad, parseData.FirstDate != null || parseData.ParsingSecondDate));
                }
                else if (ceMatch.Groups.Count == 1 && ceMatch.Groups[0].Value.Equals(bcad))
                {
                    if (AncientDateTypeWarning && !_commonEra)
                        parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.WrongAncientDateFormat, bcad, parseData.FirstDate != null || parseData.ParsingSecondDate));
                }
            }
            return isBc;
        }

        protected int GetDateYear(Match match, int yearIndex, ref ParseData parseData, int month, int day)
        {
            //bool yearZeroError = false;
            if (yearIndex > 0)
            {
                int year=0;
                int doubleYear=0;
                string yearStr = match.Groups[yearIndex].Value;
                int slashIndex = yearStr.IndexOf("/"); // DO NOT TRANSLATE
                if (slashIndex > -1)
                {
                    year = Convert.ToInt32(yearStr.Substring(0, slashIndex));
                    doubleYear = Convert.ToInt32(yearStr.Substring(slashIndex+1));
                    if (doubleYear < 10)
                    {
                        if (doubleYear == 0)
                            doubleYear = 10;
                        doubleYear = (year / 10)*10 + doubleYear;
                    }
                    else if (doubleYear < 100)
                    {
                        doubleYear = (year / 100)*100 + doubleYear;
                    }
                    if (doubleYear != year + 1)
                    {
                        parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.InvalidDoubleDate, yearStr.Trim(), parseData.FirstDate != null || parseData.ParsingSecondDate));
                    }
                    parseData.Qualifier |= DateModifier.DoubleDate;
                    year++; // storing the second year, not the first for double dates
                }
                else
                {
                    year = Convert.ToInt32(yearStr);
                }
                string bcad = match.Groups.Count > yearIndex + 1 ? match.Groups[match.Groups.Count - 1].Value : ""; // DO NOT TRANSLATE
                bool isBc = IsBC(ref parseData, bcad);
                if (isBc)
                {
                    year = -year;
                }
                else
                {
                    if (yearIndex > 0)
                    {
                        if (match.Groups[yearIndex].Value.Length == 2)
                        {
                            //throw new DateParseException(new DateParseError(DateParseErrorType.TwoDigitYear, (match.Groups[yearIndex].Value + " " + bcad).Trim())); // DO NOT TRANSLATE
                            if (TwoDigitYearAsCurrentCentury == null)
                                parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.TwoDigitYear, (match.Groups[yearIndex].Value + " " + bcad).Trim(), parseData.FirstDate != null || parseData.ParsingSecondDate)); // DO NOT TRANSLATE
                            else if (TwoDigitYearAsCurrentCentury != false)
                            {
                                int nowYear = DateTime.Now.Year;
                                if ((nowYear % 100) < year)
                                    year += (nowYear - (nowYear % 100)) - 100;
                                else
                                    year += (nowYear - (nowYear % 100));
                            }
                        }
                        else if (match.Groups[yearIndex].Value.Length == 1)
                        {
                            //throw new DateParseException(new DateParseError(DateParseErrorType.OneDigitYear, (match.Groups[yearIndex].Value + " " + bcad).Trim())); // DO NOT TRANSLATE
                            parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.OneDigitYear, (match.Groups[yearIndex].Value + " " + bcad).Trim(), parseData.FirstDate != null || parseData.ParsingSecondDate)); // DO NOT TRANSLATE
                        }
                        //if (year == 0)
                        //{
                        //    //throw new DateParseException(new DateParseError(DateParseErrorType.YearZero, (match.Groups[yearIndex].Value + " " + bcad).Trim())); // DO NOT TRANSLATE
                        //    parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.YearZero, (match.Groups[yearIndex].Value + " " + bcad).Trim(), parseData.FirstDate != null)); // DO NOT TRANSLATE
                        //    yearZeroError = true;
                        //}
                    }
                }
                //if (yearIndex > 0)
                //{
                //    if (year == 0 && !yearZeroError)
                //    {
                //        //throw new DateParseException(new DateParseError(DateParseErrorType.YearZero, (match.Groups[yearIndex].Value + " " + bcad).Trim(), parseData.FirstDate != null)); // DO NOT TRANSLATE
                //        parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.YearZero, (match.Groups[yearIndex].Value + " " + bcad).Trim(), parseData.FirstDate != null)); // DO NOT TRANSLATE
                //    }
                //}
                if (year < -4713 || year > 6000)
                {
                    //throw new DateParseException(new DateParseError(DateParseErrorType.YearOutOfRange, (match.Groups[yearIndex].Value + " " + bcad).Trim())); // DO NOT TRANSLATE
                    parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.YearOutOfRange, (match.Groups[yearIndex].Value + " " + bcad).Trim(), parseData.FirstDate != null || parseData.ParsingSecondDate)); // DO NOT TRANSLATE
                }
                if (year > 0 && year <= DoubleDateCutoffYear && 
                    month > 0 && month < 4)
                {
                    if ((parseData.Qualifier & DateModifier.DoubleDate) != DateModifier.DoubleDate)
                    {
                        if (month < 3 || (month == 3 && day < 25))
                        {
                            if (AutoCreateDoubleDates == null && !_ignoreDoubleDates)
                            {
                                parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.DoubleDateAmbiguous, yearStr, parseData.FirstDate != null || parseData.ParsingSecondDate));
                                //parseData.Qualifier |= DateModifier.DoubleDate;
                            }
                            if (_ignoreDoubleDates || AutoCreateDoubleDates == true)
                                parseData.Qualifier |= DateModifier.DoubleDate;
                        }
                    }
                }
                if ((year < 0 || year > DoubleDateCutoffYear) &&
                    (parseData.Qualifier & DateModifier.DoubleDate) == DateModifier.DoubleDate)
                {
                    parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.InvalidDoubleDate, yearStr, parseData.FirstDate != null || parseData.ParsingSecondDate));
                    parseData.Qualifier &= ~DateModifier.DoubleDate;
                }
                else if ((year > 0 && year < DoubleDateCutoffYear) && !(month < 3 || (month == 3 && day < 26)) &&
                    (parseData.Qualifier & DateModifier.DoubleDate) == DateModifier.DoubleDate)
                {
                    parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.InvalidDoubleDate, parseData.DateStr, parseData.FirstDate != null || parseData.ParsingSecondDate));
                    parseData.Qualifier &= ~DateModifier.DoubleDate;
                }
                return year;
            }
            return 0;
        }

        protected uint? GetDate(Match match, int dayIndex, int monthIndex, int yearIndex, bool monthIsNumber, int qtr, ref ParseData parseData)
        {
            uint retVal = 0;
            if (match.Groups.Count > 1)
            {
                int day = GetDateDay(match, dayIndex);
                int month = GetDateMonth(match, monthIndex, monthIsNumber, ref parseData);
                if (DayBeforeMonth && dayIndex != 0 && monthIsNumber && !(dayIndex==3 && monthIndex==2 && yearIndex==1))
                {
                    int newMonth = day;
                    day = month;
                    month = newMonth;
                }

                if (month > 12 || month < 0)// || (month == 0 && monthIndex > 0))
                {
                    //throw new DateParseException(new DateParseError(DateParseErrorType.MonthOutOfRange, month.ToString()));
                    parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.MonthOutOfRange, month.ToString(), parseData.FirstDate != null || parseData.ParsingSecondDate));
                    if ((day < 0 || day > 31))// || (day == 0 && dayIndex > 0))
                    {
                        parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.DayOutOfRange, day.ToString(), parseData.FirstDate != null || parseData.ParsingSecondDate));
                        day = 0;
                    }
                }
                else if (month > 0 && day > MonthMaxDays[month - 1] || day < 0)// || (day == 0 && dayIndex > 0))
                {
                    //throw new DateParseException(new DateParseError(DateParseErrorType.DayOutOfRange, day.ToString()));
                    parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.DayOutOfRange, day.ToString(), parseData.FirstDate != null || parseData.ParsingSecondDate));
                    day = 0;
                }

                int year = GetDateYear(match, yearIndex, ref parseData, month, day);

                if (month == 2 && day == 29 && !IsLeapYear(year))
                {
                    //throw new DateParseException(new DateParseError(DateParseErrorType.DayOutOfRange, day.ToString()));
                    parseData.DateParseErrorInfo.Add(new DateParseErrorInfo(DateParseErrorType.NotLeapYear, year.ToString(), parseData.FirstDate != null || parseData.ParsingSecondDate));
                    day = 0;
                }
                if (qtr != 0 && match.Groups[qtr].Value.Length > 0)
                {
                    parseData.Qualifier |= DateModifier.Quarter;
                }
                if (year != 0 || month != 0 || day != 0)
                {
                    if (month == 0)
                        day = 0;
                    retVal = (uint)SDNDate.Encode(year, month, day, parseData.Qualifier);
                }
                parseData.Qualifier = 0;

            }
            return retVal;
        }

        protected static bool IsReasonableDate(uint? date)
        {
            return date != null && date > 100;
        }

        protected static bool SetDate(uint? date, ref uint? dateOut)
        {
            if (dateOut == null)
                dateOut = (uint)date;
            return dateOut == date && date != 0;
        }

        protected string ParseDate(string dateStr, Regex regex, int dayIndex, int monthIndex, int yearIndex, int endYearIndex, bool monthIsNumber, int qtr, ref ParseData parseData)
        {
            MatchCollection matches = regex.Matches(dateStr);

            if (matches.Count > 0)
            {
                uint? date = GetDate(matches[0], dayIndex, monthIndex, yearIndex, monthIsNumber, qtr, ref parseData);
                if (IsReasonableDate(date))
                {
                    int dateIndex = dateStr.IndexOf(matches[0].Value);
                    dateStr = dateStr.Substring(0, dateIndex) + dateStr.Substring(dateIndex + matches[0].Length);
                }
                else
                {
                    date = 0;
                }
                uint? firstDate = parseData.FirstDate;
                uint? secondDate = parseData.SecondDate;
                if (!SetDate(date, ref firstDate))
                    SetDate(date, ref secondDate);
                else
                {
                    if (matches.Count > 1)
                    {
                        date = GetDate(matches[1], dayIndex, monthIndex, yearIndex, monthIsNumber, qtr, ref parseData);
                        if (IsReasonableDate(date) && secondDate == null)
                        {
                            // mask out the modifier because it should only apply to the beginning date of the range
                            secondDate = (date & ~Date.PROXIMITY_MASK); // mask off visual modifiers
                            dateStr = ""; // DO NOT TRANSLATE
                        }
                    }
                    else if (matches[0].Groups[endYearIndex].Value.Length > 0 && endYearIndex != 0)
                    {
                        int value = Convert.ToInt32(matches[0].Groups[endYearIndex].Value);
                        if (value < 100)
                        {
                            int? year = new SDNDate((uint)firstDate).Year;
                            if (year == null) year = 0;
                            int firstYearCentury = ((short)year) / 100;

                            secondDate = SDNDate.Encode((short?)(value + (firstYearCentury * 100)), null, null, DateModifier.None);
                        }
                        else
                        {
                            secondDate = SDNDate.Encode((short?)value, null, null, DateModifier.None);
                        }
                    }
                }
                parseData.FirstDate = firstDate;
                parseData.SecondDate = secondDate;
            }
            return dateStr;
        }

        protected int GetMonth(string monthStr)
        {
            monthStr = monthStr.ToLower();
            for (int i = 0; i < Months.Length/3; i++)
            {
                if (monthStr.Equals(Months[i, 0]) ||
                    monthStr.Equals(Months[i, 1]))
                    return i + 1;
                if (Months[i,2] != null && monthStr.Equals(Months[i,2]))
                    return i + 1;
            }
            return 0;
        }

        protected void InitializeWordsToQualifier(string list, DateModifier modifier)
        {
            string[] words = list.Split(',');
            foreach (string word in words)
            {
                WordsToQualifier[word] = modifier;
            }
        }

        protected void InitializeWordsToQualifier()
        {
            if (WordsToQualifier == null)
            {
                WordsToQualifier = new Dictionary<string, DateModifier>();

                InitializeWordsToQualifier(DateParserStrings.ResourceManager.GetString("Before", _culture), DateModifier.Before);
                InitializeWordsToQualifier(DateParserStrings.ResourceManager.GetString("About", _culture), DateModifier.About);
                InitializeWordsToQualifier(DateParserStrings.ResourceManager.GetString("After", _culture), DateModifier.After);
                InitializeWordsToQualifier(DateParserStrings.ResourceManager.GetString("Calculated", _culture), DateModifier.Calculated);

                //TODO: Get with Mike on these and figure out how to handle them
                InitializeWordsToQualifier(DateParserStrings.ResourceManager.GetString("Between", _culture), DateModifier.None);
                WordsToQualifier["wft est"] = DateModifier.None;  // DO NOT TRANSLATE
            }
        }

        protected static bool IsLeapYear(int year)
        {
            bool retVal = false;
            if(year % 4 == 0)
            {
                retVal = true;
                if (year % 100 == 0)
                {
                    if (year % 400 != 0)
                    {
                        retVal = false;
                    }
                }
            }
            return retVal;
        }

        private static void SetErrorsInPriorityOrder(ParseData parseData)
        {
            if (parseData.DateParseErrorInfo.Count > 1)
            {
                IList<DateParseErrorInfo> orderedList = new List<DateParseErrorInfo>(parseData.DateParseErrorInfo.Count);
                int startCount = parseData.DateParseErrorInfo.Count;
                for (int i = 0; i < startCount; i++)
                {
                    int index = 0;
                    for (int n = 1; n < parseData.DateParseErrorInfo.Count; n++)
                    {
                        if (parseData.DateParseErrorInfo[index].Reason < parseData.DateParseErrorInfo[n].Reason)
                        {
                            index = n;
                        }
                    }
                    if (orderedList.Count == 0 || 
                        orderedList[orderedList.Count-1].Reason != parseData.DateParseErrorInfo[index].Reason ||
                        orderedList[orderedList.Count-1].SecondDateError != parseData.DateParseErrorInfo[index].SecondDateError)
                        orderedList.Add(parseData.DateParseErrorInfo[index]);
                    parseData.DateParseErrorInfo.RemoveAt(index);
                }
                for (int i = 0; i < orderedList.Count; i++)
                {
                    parseData.DateParseErrorInfo.Add(orderedList[i]);
                }

            }
        }

    }
    #endregion
}


