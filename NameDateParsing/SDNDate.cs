using System;
using System.Globalization;
using System.Linq;

namespace NameDateParsing
{
    /// <summary>
    /// Date represented by a serial day number
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
    /// E - Encoding type (always 0)
    ///     SDN encoding specifies a calendar agnostic encoding that stores
    ///         Day, Month, Year. It also includes extra bits for modifiers
    /// SDN - A standard number of days elapsed from a starting date (November 25, 4714 BC
    ///     in the Gregorian calendar, January 2, 4713 BC in the Julian calendar)
    /// MODIFIER - Modifies the meaning or interpretation of the date. Values can be combined,
    ///     however combining some values does not make sense
    ///     About, Before, After, Calculated, DayMissing, MonthMissing, YearMissing, IsDual
    /// </remarks>
    /// </summary>
    public class SDNDate : Date
    {
        private const int MAX_YEAR = 6770;

        /// <summary>
        /// Constructs a Date from an encoded value
        /// </summary>
        /// <param name="code">Previously encoded value</param>
        internal SDNDate(uint code)
            : base(code)
        {
            _modifier = (DateModifier)(_code.Value & MODIFIER_MASK);
        }

        /// <summary>
        /// Constructs a Date from a serial day number
        /// </summary>
        /// <param name="code">Previously encoded value</param>
        public SDNDate(uint sdn, DateModifier modifier)
            : base(Encode(sdn, modifier))
        {
            _modifier = modifier;
        }

        /// <summary>
        /// Encodes a DateTime
        /// </summary>
        /// <param name="dt">date/time to encode</param>
        /// <returns>encoded date</returns>
        public static uint Encode(DateTime dt)
        {
            return (uint)Encode(dt.Year, dt.Month, dt.Day, DateModifier.None);
        }

        /// <summary>
        /// Encodes a Serial Day Number
        /// </summary>
        /// <param name="sdn">serial day number</param>
        /// <returns>encoded date</returns>
        public static uint Encode(uint serialDayNumber)
        {
            return Encode(serialDayNumber, DateModifier.None);
        }

        public static uint Encode(uint serialDayNumber, DateModifier modifier)
        {
            return (serialDayNumber << SDN_SHIFT) | (uint)modifier;
        }

        // TODO: handle calendar better
        public static uint Encode(int? year, int? month, int? day, DateModifier modifier)
        {
            int Year, Month, Day;
            Year = (year == null || year == 0) ? -4713 : (int)year;
            Month = (month == null || month == 0) ? 1 : (int)month;
            Day = (day == null || day == 0) ? 1 : (int)day;
			uint snd = (uint)NameDateParsing.SerialDayNumber.GregorianToSdn(Year, Month, Day);
            if (year == null || year == 0)
                modifier |= DateModifier.YearMissing;
            if (month == null || month == 0)
                modifier |= DateModifier.MonthMissing;
            if (day == null || day == 0)
                modifier |= DateModifier.DayMissing;
            return (snd << SDN_SHIFT) | (uint)modifier;
        }

        private uint? _sortDate;
        internal override uint SortDate
        {
            get
            {
                if (!_sortDate.HasValue)
                {
                    if (_code == null || _code == 0) _sortDate = 0xFFFFFFFF;
                    else if (Year == null) _sortDate = (uint) Encode((int?) MAX_YEAR, Month, Day, Modifier);
                    else _sortDate = (uint) _code;
                }

                return _sortDate.Value;
            }
        }

        public static Date CalculateMean(Date[] dates)
        {
            ulong total = 0;
            ulong count = 0;
            foreach (SDNDate date in dates.OfType<SDNDate>())
            {
                uint sdn = date.SerialDayNumber;
                total += sdn;
                count++;
            }
            uint avg = (count == 0) ? 0 : (uint)(total / count);

            return new SDNDate(Encode(avg));
        }

        public override DateModifier Modifier
        {
            get
            {
                return _modifier;
            }
        }

        /// <summary>
        /// Gets the serial day number.
        /// </summary>
        /// <value>The serial day number.</value>
        public uint SerialDayNumber
        {
            get
            {
                return ((_code.Value & SDN_MASK) >> SDN_SHIFT);
            }
        }

        public override bool Equals(Date date)
        {
            if (!(date is SDNDate)) return false;

            return _code == ((SDNDate)date)._code;
        }

        protected override int? GetYear(System.Globalization.Calendar cal)
        {
            if ((((uint)Modifier) & Date.YEARMISSING_MASK) > 0) return null;

            if (cal is GregorianCalendar)
            {
                int year, month, day;
                NameDateParsing.SerialDayNumber.SdnToGregorian((int)SerialDayNumber, out year, out month, out day);

                return year;
            }
            else if (cal is JulianCalendar)
            {
                int year, month, day;
				NameDateParsing.SerialDayNumber.SdnToJulian((int)SerialDayNumber, out year, out month, out day);

                return year;
            }

            return null;
        }

        protected override int? GetMonth(System.Globalization.Calendar cal)
        {
            if ((((uint)Modifier) & Date.MONTHMISSING_MASK) > 0) return null;

            if (cal is GregorianCalendar)
            {
                int year, month, day;
				NameDateParsing.SerialDayNumber.SdnToGregorian((int)SerialDayNumber, out year, out month, out day);

                return month;
            }
            else if (cal is JulianCalendar)
            {
                int year, month, day;
				NameDateParsing.SerialDayNumber.SdnToJulian((int)SerialDayNumber, out year, out month, out day);

                return month;
            }

            return null;
        }

        protected override int? GetDay(System.Globalization.Calendar cal)
        {
            if ((((uint)Modifier) & Date.DAYMISSING_MASK) > 0) return null;

            if (cal is GregorianCalendar)
            {
                int year, month, day;
				NameDateParsing.SerialDayNumber.SdnToGregorian((int)SerialDayNumber, out year, out month, out day);

                return day;
            }
            else if (cal is JulianCalendar)
            {
                int year, month, day;
				NameDateParsing.SerialDayNumber.SdnToJulian((int)SerialDayNumber, out year, out month, out day);

                return day;
            }

            return null;
        }
    }
}
