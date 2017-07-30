using System;

namespace NameDateParsing
{
    /// <summary>
    /// Defines an interface that objects can implement in order to convert them to a string representation
    /// </summary>
    public interface IFormattableEx : IFormattable
    {
        /// <summary>
        /// Converts the object to a string representation
        /// </summary>
        /// <returns></returns>
        string ToString();

        /// <summary>
        /// Converts the object to a string representation given a format specifier
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        string ToString(string format);

        /// <summary>
        /// Converts the object to a string representation given a format provider
        /// <remarks>
        /// Use this method when the object doesn't directly support the formatting you require
        /// </remarks>
        /// </summary>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        string ToString(IFormatProvider formatProvider);

        FormatInfo Formatter { get; set; }
    }

    /// <summary>
    /// Defines an interface that a formatter can implement to change it's default behavior
    /// </summary>
    public interface ICustomFormatterEx : ICustomFormatter
    {
        /// <summary>
        /// Gets or sets the default format string for the object 
        /// </summary>
        string DefaultFormat { get; set; }
    }

}
