using System;
using System.Globalization;

namespace NameDateParsing
{
    /// <summary>
    /// Represents a genealogical date
    /// <remarks>
    /// Date Encoding:
    /// 0 0 0 0 | 0 0 0 0 | 0 0 0 0 | 0 0 0 0 || 0 0 0 0 | 0 0 0 0 | 0 0 0 0 | 0 0 0 0
    /// | \____________________________________________________/     | | | |   | \___/
    ///                                                              | | | |   |   | 
    /// E     SDN (Serial Day Number/Julian Day 1 to 4194303)        | | | |   |  About/Before/After/Calculated
    ///                                                              | | | |   Quarter Date (Qtr)
    ///                                                              | | | Dual Date
    ///                                                              | | No Year
    ///                                                              | No Month
    ///                                                              No Day
    ///
    /// E - Encoding type (0 = Serial Day Number/SDN format, 1 = Enumerated)
    ///     SDN encoding specifies a calendar agnostic encoding that stores
    ///         Day, Month, Year. It also includes extra bits for modifiers
    ///     Enumerated format stores a list of possible date values (such as STILLBORN)
    /// SDN - A standard number of days elapsed from a starting date (November 25, 4714 BC
    ///     in the Gregorian calendar, January 2, 4713 BC in the Julian calendar)
    /// MODIFIER - Modifies the meaning or interpretation of the date. Values can be combined,
    ///     however combining some values does not make sense
    ///     About, Before, After, Calculated, DayMissing, MonthMissing, YearMissing, IsDual
    /// </remarks>
    /// </summary>
    public abstract class Date : IDate, IEquatable<Date>, IComparable<Date>
    {
        private static FormatInfo s_formatter = DateFormatInfo.Default;

        protected uint? _code;
        protected DateModifier _modifier;

        #region Constructors

        /// <summary>
        /// Constructs a Date from an encoded value
        /// </summary>
        /// <param name="code">Previously encoded value</param>
        protected Date(uint? code)
        {
            _code = code;
        }

        internal abstract uint SortDate { get; }

        #region Factory Methods

        /// <summary>
        /// Constructs a date from an unparsed string
        /// </summary>
        /// <remarks>These types of dates are sorted after all others and will be excluded from date calculations</remarks>
        /// <param name="unparsed">Unparsed string</param>
        public static Date CreateInstance(string unparsed)
        {
            return new TextDate(unparsed);
        }

        public static Date CreateInstance(uint? code)
        {
            return CreateInstance(code, null);
        }

        public static Date CreateInstance(uint? code1, uint? code2)
        {
            if (!code1.HasValue) return null;
            if (code1 == 0) return null;

            if (code2.HasValue)
            {
                System.Diagnostics.Debug.Assert((code1 & ENCODING_MASK) == ENCODING_SDN);
                System.Diagnostics.Debug.Assert((code2 & ENCODING_MASK) == ENCODING_SDN);
                
                return new DateRange((uint)code1, (uint)code2);
            }
            else if ((code1 & ENCODING_MASK) == ENCODING_SDN)
            {
                return new SDNDate((uint)code1);
            }
            else
            {
                return new KeywordDate((uint)code1);
            }
        }

        #endregion

        internal const uint MODIFIER_MASK     = 0x000001FF;
        internal const uint PROXIMITY_MASK    = 0x00000007; // mask for about/before/after/calculated
        internal const uint SDN_MASK          = 0x7FFFFE00;
        internal const uint ENCODING_MASK     = 0x80000000;
        internal const uint DAYMISSING_MASK   = 0x00000080;
        internal const uint MONTHMISSING_MASK = 0x00000040;
        internal const uint YEARMISSING_MASK  = 0x00000020;
        internal const uint DUALDATE_MASK     = 0x00000010;
        internal const uint QUARTER_DATE_MASK = 0x00000008;

        internal const uint MODIFIER_NONE       = 0x00000000;
        internal const uint MODIFIER_BEFORE     = 0x00000001;
        internal const uint MODIFIER_AFTER      = 0x00000002;
        internal const uint MODIFIER_ABOUT      = 0x00000003;
        internal const uint MODIFIER_CALCULATED = 0x00000004;

        internal const uint ENCODING_SDN        = 0x00000000;
        internal const uint ENCODING_KEYWORD    = 0x80000000;

        internal const uint KEYWORD_MASK        = 0x0000ffff;

        internal const int SDN_SHIFT = 9;

        #region Encoders


        /// <summary>
        /// Encodes the parts of a date
        /// </summary>
        /// <param name="year">Year to encode</param>
        /// <param name="month">Month to encode</param>
        /// <param name="day">Day to encode</param>
        /// <param name="modifier">DateModifier to encode</param>
        /// <returns></returns>

        public static uint? Encode(Date date)
        {
            return date._code;
        }

        #endregion

        #region IDate Members

        /// <summary>
        /// Date modifier
        /// </summary>
        public virtual DateModifier Modifier
        {
            get 
            {
                return DateModifier.None;
            }
        }

        public int? Year
        {
            get 
            {
                if ((_modifier & DateModifier.YearMissing) > 0) return null;

                return GetYear(new GregorianCalendar(GregorianCalendarTypes.Localized)); 
            }
        }

        public int? Month
        {
            get 
            { 
                if ((_modifier & DateModifier.MonthMissing) > 0) return null;

                return GetMonth(new GregorianCalendar(GregorianCalendarTypes.Localized)); 
            }
        }

        public int? Day
        {
            get 
            { 
                if ((_modifier & DateModifier.DayMissing) > 0) return null;

                return GetDay(new GregorianCalendar(GregorianCalendarTypes.Localized)); 
            }
        }

        /// <summary>
        /// Year (null if no year specified)
        /// </summary>
        protected virtual int? GetYear(Calendar cal)
        {
            return null;
        }

        /// <summary>
        /// Month (null if no month specified)
        /// </summary>
        protected virtual int? GetMonth(Calendar cal)
        {
            return null;
        }

        /// <summary>
        /// Day (null if no day specified)
        /// </summary>
        protected virtual int? GetDay(Calendar cal)
        {
            return null;
        }

        #endregion

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Date);
        }

        #region IEquatable<Date> Members

        public abstract bool Equals(Date date);

        #endregion

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override int GetHashCode()
        {
            return (_code.HasValue) ? (int)_code.Value : 0;
        }

        #region IComparable<Date> Members

        /// <summary>
        /// Compares this Date with another Date object.
        /// </summary>
        /// <param name="other">A Date to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the Dates being compared.
        /// </returns>
        public virtual int CompareTo(Date other)
        {
            if (other == null) return -1;
            
            // Compare the date
            int val = SortDate.CompareTo(other.SortDate);

            // Final modifier check (SortDate1 only)
            if ((_modifier != DateModifier.None) || (other._modifier != DateModifier.None))
            {
                // Strip off the proximity modifiers about | before | after
                uint v1 = SortDate & ~PROXIMITY_MASK;
                uint v2 = other.SortDate & ~PROXIMITY_MASK;
                DateModifier m1 = (DateModifier)(SortDate & PROXIMITY_MASK);
                DateModifier m2 = (DateModifier)(other.SortDate & PROXIMITY_MASK);

                // if the dates are equal, compare based on the modifiers
                if (0 == v1.CompareTo(v2))
                {
                    if ((m1 == DateModifier.None) && (m2 == DateModifier.Before))
                        val = 1;
                    else if ((m1 == DateModifier.None) && (m2 == DateModifier.After))
                        val = -1;

                    else if ((m1 == DateModifier.Before) && (m2 == DateModifier.None))
                        val = -1;
                    else if ((m1 == DateModifier.Before) && (m2 == DateModifier.After))
                        val = -1;

                    else if ((m1 == DateModifier.After) && (m2 == DateModifier.None))
                        val = 1;
                    else if ((m1 == DateModifier.After) && (m2 == DateModifier.Before))
                        val = 1;
                }
            }

            return val;
        }

        #endregion

        #region IComparable<IDate> Members

        /// <summary>
        /// Compares this Date with another Date object.
        /// </summary>
        /// <param name="other">A Date to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the Dates being compared.
        /// </returns>
        int IComparable<IDate>.CompareTo(IDate other)
        {
            int val = ((IComparable<Date>)this).CompareTo((Date)other);
            if (val == 0 && other is IRange<IDate>)
            {
                if (!(this is IRange<IDate>)) return -1;
                else
                {
                    IRange<IDate> otherRange = other as IRange<IDate>;
                    IRange<IDate> thisRange = this as IRange<IDate>;
                    return ((IComparable<Date>)thisRange.End).CompareTo((Date)otherRange.End);
                }
            }

            return val;
        }

        #endregion

        public static FormatInfo DefaultFormatter
        {
            get
            {
                return s_formatter;
            }
            set
            {
                System.Diagnostics.Debug.Assert(value != null);
                s_formatter = value;
            }
        }

        #region IFormattableEx Members

        public override string ToString()
        {
            return s_formatter.Format(null, this, null);
        }

        public string ToString(string format)
        {
            return s_formatter.Format(format, this, null);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return s_formatter.Format(null, this, formatProvider);
        }

        public FormatInfo Formatter
        {
            get
            {
                return s_formatter;
            }
            set
            {
                System.Diagnostics.Debug.Assert(false, "Operation is not supported");
            }
        }

        #endregion

        #region IFormattable Members

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return s_formatter.Format(format, this, formatProvider);
        }

        #endregion
    }

    #endregion
}
