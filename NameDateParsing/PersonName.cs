using System;
using System.Text.RegularExpressions;

namespace NameDateParsing
{
    /// <summary>
    /// Represents a person's name
    /// </summary>
    public class PersonName : IPersonName
    {
        static Regex s_parser = new Regex(@"^(?<given>.*)(\s*)/(?<family>.*)/(\s*)(?<suffix>.*)$", RegexOptions.Compiled); // DO NOT TRANSLATE
        protected FormatInfo _formatter = PersonNameFormatInfo.Default;

        private string _text;
        private string _givenName;
        private string _familyName;
        private string _nameSuffix;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:PersonName"/> class.
        /// </summary>
        /// <param name="text">Parsed or unparsed name</param>
        /// <param name="given">Given name(s)</param>
        /// <param name="family">Family name</param>
        /// <param name="suffix">Name suffix</param>
        public PersonName(string given, string family, string suffix)
        {
            _text = Encode(given, family, suffix);
            _givenName = given;
            _familyName = family;
            _nameSuffix = suffix;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:PersonName"/> class.
        /// </summary>
        /// <param name="s">Encoded name (encode using PersonName.Encode)</param>
        public PersonName(string s)
        {
            _text = s;
            Decode(s, out _givenName, out _familyName, out _nameSuffix);
        }

        public PersonName(IPersonName name)
        {
            System.Diagnostics.Debug.Assert(name != null);
            _text = Encode(name);
            Decode(_text, out _givenName, out _familyName, out _nameSuffix);
        }

        public static readonly PersonName Unknown = Decode(string.Empty);

        /// <summary>
        /// Encodes name parts into a string for storage in the database
        /// </summary>
        /// <param name="givenName">Given name</param>
        /// <param name="familyName">Familiy name</param>
        /// <param name="nameSuffix">Name suffix</param>
        /// <returns></returns>
        public static string Encode(string givenName, string familyName, string nameSuffix)
        {
            return string.Format("{0} /{1}/ {2}", givenName, familyName, nameSuffix).Trim(); // DO NOT TRANSLATE
        }

        public static string Encode(IPersonName name)
        {
            return name.Text;
        }

        /// <summary>
        /// Encodes name parts into a string for storage in the database
        /// </summary>
        /// <param name="givenName">Given name</param>
        /// <param name="familyName">Familiy name</param>
        /// <param name="nameSuffix">Name suffix</param>
        /// <returns></returns>
        public static string EncodeReverse(string givenName, string familyName, string nameSuffix)
        {
            return string.Format("/{0}/ {1} {2}", familyName, givenName, nameSuffix).Trim(); // DO NOT TRANSLATE
        }

        /// <summary>
        /// Decodes a name string into its individual parts
        /// </summary>
        /// <param name="s">string to decode</param>
        /// <param name="givenName">Given name</param>
        /// <param name="familyName">Family name</param>
        /// <param name="nameSuffix">Name suffix</param>
        public static void Decode(string s, out string givenName, out string familyName, out string nameSuffix)
        {
            givenName = familyName = nameSuffix = null;
            if (!string.IsNullOrEmpty(s))
            {
                //string[] parts = s.Split('/');
                //if (parts.Length == 3)
                //{
                //    givenName = parts[0].Trim();
                //    familyName = parts[1].Trim();
                //    nameSuffix = parts[2].Trim();
                //}

                Match m = s_parser.Match(s);
                if (m.Success)
                {
                    givenName = m.Groups["given"].Value.Trim(); // DO NOT TRANSLATE
                    familyName = m.Groups["family"].Value.Trim(); // DO NOT TRANSLATE
                    nameSuffix = m.Groups["suffix"].Value.Trim(); // DO NOT TRANSLATE
                }
            }
        }

        /// <summary>
        /// Decodes a person's name into an object representing that name
        /// </summary>
        /// <param name="s">string to decode</param>
        /// <returns>Object representing the person's name</returns>
        public static PersonName Decode(string s)
        {
            return new PersonName(s);
        }

        /// <summary>
        /// Encodes this instance into a string
        /// </summary>
        /// <returns>Encoded string</returns>
        public string Encode()
        {
            return Encode(_givenName, _familyName, _nameSuffix);
        }

        /// <summary>
        /// Encodes this instance into a string
        /// </summary>
        /// <returns>Encoded string</returns>
        public string EncodeReverse()
        {
            return EncodeReverse(_givenName, _familyName, _nameSuffix);
        }

        #region IPersonName Members

        /// <summary>
        /// Gets or sets the person's family name
        /// </summary>
        /// <value></value>
        public string FamilyName
        {
            get
            {
                return _familyName;
            }
            set
            {
                _familyName = value;
                _text = Encode();
            }
        }

        /// <summary>
        /// Gets or sets the person's given name
        /// </summary>
        public string GivenName
        {
            get
            {
                return _givenName;
            }
            set
            {
                _givenName = value;
                _text = Encode();
            }
        }

        /// <summary>
        /// Gets or sets the person's name suffix
        /// </summary>
        public string NameSuffix
        {
            get
            {
                return _nameSuffix;
            }
            set
            {
                _nameSuffix = value;
                _text = Encode();
            }
        }

        #endregion

        #region IText Members

        /// <summary>
        /// Gets or sets the encoded (or unparsed) full name
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                Decode(_text, out _givenName, out _familyName, out _nameSuffix);
            }
        }

        #endregion


        #region IFormattableEx Members

        public FormatInfo Formatter
        {
            get { return _formatter; }
            set { _formatter = value; }
        }

        public override string ToString()
        {
            return _formatter.Format(null, this, null);
        }

        public string ToString(string format)
        {
            return _formatter.Format(format, this, null);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return _formatter.Format(null, this, formatProvider);
        }

        #endregion

        #region IFormattable Members

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return _formatter.Format(format, this, formatProvider);
        }

        #endregion
    }
}
