using System;

namespace NameDateParsing
{
    /// <summary>
    /// Parts of a person name that may have been modified
    /// <remarks>
    /// Used with ModifiedEventArgs in a Modified event handler
    /// </remarks>
    /// </summary>
    [Flags]
    public enum PersonNameModifiedFlags
    {
        None = 0x00,
        FamilyNameModified = 0x01,
        GivenNameModified = 0x02,
        NameSuffixModified = 0x04,
        All = 0x07
    }

    /// <summary>
    /// Represents the name of a person
    /// </summary>
    public interface IPersonName : IText, IFormattableEx
    {
        /// <summary>
        /// Person's family name (surname)
        /// </summary>
        string FamilyName { get; }
        
        /// <summary>
        /// Person's Given Name(s)
        /// </summary>
        string GivenName { get; }
        
        /// <summary>
        /// Person's Name Suffix (Jr, Sr, III, etc)
        /// </summary>
        string NameSuffix { get; }
    }
}
