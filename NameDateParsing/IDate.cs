using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace NameDateParsing
{
    public enum DateType
    {
        SerialDayNumber = 0, Enumerated = 1
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum DateModifier : int
    {
        /// <summary>
        /// No modifiers; the date is exact
        /// </summary>
        None = 0x0000,
        
        /// <summary>
        /// 
        /// </summary>
        Before = 0x0001,
        
        /// <summary>
        /// 
        /// </summary>
        After = 0x0002,
        
        /// <summary>
        /// 
        /// </summary>
        About = 0x0003,

        /// <summary>
        /// 
        /// </summary>
        Quarter = 0x0008,

        /// <summary>
        /// 
        /// </summary>
        YearMissing = 0x0020,

        /// <summary>
        /// 
        /// </summary>
        MonthMissing = 0x0040,

        /// <summary>
        /// 
        /// </summary>
        DayMissing = 0x0080,
        
        /// <summary>
        /// 
        /// </summary>
        DoubleDate = 0x0010,

        /// <summary>
        /// 
        /// </summary>
        Calculated = 0x0100
    }

    /// <summary>
    /// Represents a genealogical date
    /// </summary>
    public interface IDate : IComparable<IDate>, IFormattableEx
    {
        /// <summary>
        /// Gets the fuzziness modifier
        /// </summary>
        DateModifier Modifier { get; }

        /// <summary>
        /// Gets the year
        /// </summary>
        int? Year { get; }

        /// <summary>
        /// Gets the month
        /// </summary>
        int? Month { get; }

        /// <summary>
        /// Gets the day
        /// </summary>
        int? Day { get; }
    }
}
