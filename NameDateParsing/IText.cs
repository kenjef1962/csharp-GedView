using System;

namespace NameDateParsing
{
    /// <summary>
    /// Represents parsed or unparsed text entered by the user
    /// </summary>
    public interface IText : IFormattableEx
    {
        /// <summary>
        /// The parsed or unparsed text
        /// </summary>
        string Text { get; set; }
    }
}
