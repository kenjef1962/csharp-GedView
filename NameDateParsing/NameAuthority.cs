using System;

namespace NameDateParsing
{
    /// <summary>
    /// Argument passed to an event handler when there is a name parsing error
    /// </summary>
    public class NameParseEventArgs : EventArgs
    {
        private NameParseError _error;

        internal NameParseEventArgs(NameParseError error)
        {
            _error = error;
        }

        public NameParseError Error
        {
            get
            {
                return _error;
            }
        }
    }

    /// <summary>
    /// Authority responsible for parsing person names
    /// </summary>
    public class NameAuthority
    {
        /// <summary>
        /// Event that gets fired when there is a failure parsing a person's name
        /// </summary>
        public event EventHandler<NameParseEventArgs> ParseFailure;

        /// <summary>
        /// Parses a string into a person's name
        /// </summary>
        /// <param name="name">string to parse</param>
        /// <returns>object representing the parsed name</returns>
        [System.Diagnostics.DebuggerStepThrough] // this is so we don't have to stop on all the possible exceptions
        public IPersonName ParseName(string name)
        {
            NameParser parser = new NameParser();
            IPersonName pname = null;

            try
            {
                NameParseErrorType errType = NameParseErrorType.None;

                if (name == null || name.Trim().Length == 0)
                {
                    errType |= NameParseErrorType.NameEmpty;
                }
                else
                {
                    try
                    {
                        parser.ParseName(name);
                    }
                    catch (Exception e)
                    {
                        throw new NameParseException(new NameParseError(NameParseErrorType.Unknown), "Unknown Error", e); // DO NOT TRANSLATE
                    }
                }

                if (parser.Title != string.Empty)
                {
                    errType |= NameParseErrorType.TitleFound;
                }
                if (parser.NickName != string.Empty)
                {
                    errType |= NameParseErrorType.NickNameFound;
                }
                if (parser.Given == null)
                {
                    errType |= NameParseErrorType.GivenNameMissing;
                }
                if (parser.Family == null)
                {
                    errType |= NameParseErrorType.FamilyNameMissing;
                }

                if (NameParseErrorType.None != errType)
                {
                    throw new NameParseException(new NameParseError(errType, parser));
                }

                pname = new PersonName(parser.Given, parser.Family, parser.Suffix);
            }
            catch (NameParseException ex)
            {
                if (ParseFailure != null)
                {
                    ParseFailure(this, new NameParseEventArgs(ex.Error));
                    pname = ex.Error.ParsedName;
                }
                else
                {
                    // DO NOT remove this throw statement!
                    // If you don't want an exception thrown then hook up an event handler
                    throw;
                }
            }

            return pname;
        }
    }
}
