using System;

namespace NameDateParsing
{
    /// <summary>
    /// Handles formatting of a person's name
    /// </summary>
    public class PersonNameFormatInfo : FormatInfo
    {
        /// <summary>
        /// characters to strip from begin & end of the name
        /// </summary>
        private static char[] s_trim = { ' ', ',' };

        public static PersonNameFormatInfo Default
        {
            get
            {
                return GetDefault<IPersonName, PersonNameFormatInfo>();
            }
        }

        protected bool _caps = false;

        // Standard Format strings
        //
        /// <summary>
        /// Formats the name in reverse order (family name first)
        /// </summary>
        public const string Reverse = "R"; // DO NOT TRANSLATE
        /// <summary>
        /// Formats the name as First Last
        /// </summary>
        public const string FirstLast = "FL";
        /// <summary>
        /// Formats the name on two lines as First\nLast
        /// </summary>
        public const string FirstSlashLast = "F/L";
        /// <summary>
        /// Formats the name as First Middle Last (same as General)
        /// </summary>
        public const string FirstMiddleLast = "FML";
        /// <summary>
        /// Formats the name on two lines as First Middle\nLast
        /// </summary>
        public const string FirstMiddleSlashLast = "FM/L";
        /// <summary>
        /// Formats the name as First MI Last
        /// </summary>
        public const string FirstMLast = "FmL";
        /// <summary>
        /// Formats the name on two lines as First MI\nLast
        /// </summary>
        public const string FirstMSlashLast = "Fm/L";
        /// <summary>
        /// Formats the name as FI Last
        /// </summary>
        public const string FLast = "fL";
        /// <summary>
        /// Formats the name on two lines as FI\nLast
        /// </summary>
        public const string FSlashLast = "f/L";
        /// <summary>
        /// Formats the name as FI MI Last
        /// </summary>
        public const string FMLast = "fmL";
        /// <summary>
        /// Formats the name on two lines as FI MI\nLast
        /// </summary>
        public const string FMSlashLast = "fm/L";
        /// <summary>
        /// Formats the name as Last
        /// </summary>
        public const string Last = "L";
        /// <summary>
        /// Formats the name in reverse order (same as Reverse)
        /// </summary>
        public const string LastFirstMiddle = "LFM";
        /// <summary>
        /// Formats the name on two lines in reverse order as Last\nFirst Middle
        /// </summary>
        public const string LastSlashFirstMiddle = "L/FM";

        // Custom formatting tokens
        //
        /// <summary>
        /// FormatID for the given name
        /// </summary>
        public const string Given = "ng"; // DO NOT TRANSLATE
        /// <summary>
        /// FormatID for the family name
        /// </summary>
        public const string Family = "nf"; // DO NOT TRANSLATE
        /// <summary>
        /// FormatID for the name suffix
        /// </summary>
        public const string Suffix = "nx"; // DO NOT TRANSLATE
        /// <summary>
        /// FormatID for the name with full first name and abbreviated middle name(s)
        /// </summary>
        public const string GivenAbbreviated = "ga"; // DO NOT TRANSLATE
        /// <summary>
        /// FormatID for the name with only the first word in the given name
        /// </summary>
        public const string GivenFirst = "gf"; // DO NOT TRANSLATE
        /// <summary>
        /// FormatID for the name with all words in the given name as initials
        /// </summary>
        public const string GivenInitials = "gi"; // DO NOT TRANSLATE
        /// <summary>
        /// FormatID for the name with the first word of the given name as an initial
        /// </summary>
        public const string FirstInitial = "fi"; // DO NOT TRANSLATE

        /// <summary>
        /// FormatID for the First and Middle Names
        /// </summary>
        public const string FirstMiddle = "FM"; // DO NOT TRANSLATE

        public bool FamilyNamesInAllCaps
        {
            get { return _caps; }
            set { _caps = value; }
        }

        protected override string GetCacheKey(object obj)
        {
            if (obj is IPersonName) 
            {
                return (_caps ? "1" : "0") + "_" + (obj as IPersonName).Text;
            }
            
            return base.GetCacheKey(obj);
        }

        protected override string InnerFormat(string format, object obj, IFormatProvider formatProvider)
        {
            if (obj is IPersonName)
                return FormatName(format, (IPersonName)obj, formatProvider);
            else
                return base.InnerFormat(format, obj, formatProvider);
        }

        protected virtual string FormatName(string format, IPersonName arg, IFormatProvider formatProvider)
        {
            IFormatProvider fp = formatProvider ?? this;

            string ret = string.Empty;
            switch (format)
            {
                // standard format specifiers
                case General: // full name [given] [family] [suffix]
                case FirstMiddleLast:
                    ret = string.Format(fp, "{0:ng nf nx}", arg).Trim(); // DO NOT TRANSLATE
					ret = (ret.Length == 0) ? Properties.Resources.PersonNameUnknown : ret;
                    ret = base.CollapseWhitespace(ret);
                    break;
                case Reverse: // reverse (family name first) [family], [given] [suffix]
                case LastFirstMiddle:
                    ret = string.Format(fp, "{0:nf, ng nx}", arg); // DO NOT TRANSLATE
                    ret = (ret.Length == 0) ? Properties.Resources.PersonNameUnknown : ret;
                    ret = base.CollapseWhitespace(ret);
                    break;
                case LastSlashFirstMiddle:
                    ret = string.Format(fp, "{0:nf\nng nx}", arg); // DO NOT TRANSLATE
					ret = (ret.Length == 0) ? Properties.Resources.PersonNameUnknown : ret;
                    ret = base.CollapseWhitespace(ret);
                    break;
                case FirstLast:
                    ret = string.Format(fp, "{0:gf nf nx}", arg); // DO NOT TRANSLATE
					ret = (ret.Length == 0) ? Properties.Resources.PersonNameUnknown : ret;
                    ret = base.CollapseWhitespace(ret);
                    break;
                case FirstSlashLast:
                    ret = string.Format(fp, "{0:gf\nnf nx}", arg); // DO NOT TRANSLATE
					ret = (ret.Length == 0) ? Properties.Resources.PersonNameUnknown : ret;
                    ret = base.CollapseWhitespace(ret);
                    break;
                case FirstMiddleSlashLast:
                    ret = string.Format(fp, "{0:ng\nnf nx}", arg); // DO NOT TRANSLATE
					ret = (ret.Length == 0) ? Properties.Resources.PersonNameUnknown : ret;
                    ret = base.CollapseWhitespace(ret);
                    break;
                case FirstMLast:
                    ret = string.Format(fp, "{0:ga nf nx}", arg); // DO NOT TRANSLATE
					ret = (ret.Length == 0) ? Properties.Resources.PersonNameUnknown : ret;
                    ret = base.CollapseWhitespace(ret);
                    break;
                case FirstMSlashLast:
                    ret = string.Format(fp, "{0:ga\nnf nx}", arg); // DO NOT TRANSLATE
					ret = (ret.Length == 0) ? Properties.Resources.PersonNameUnknown : ret;
                    ret = base.CollapseWhitespace(ret);
                    break;
                case FLast:
                    ret = string.Format(fp, "{0:fi nf nx}", arg); // DO NOT TRANSLATE
					ret = (ret.Length == 0) ? Properties.Resources.PersonNameUnknown : ret;
                    ret = base.CollapseWhitespace(ret);
                    break;
                case FSlashLast:
                    ret = string.Format(fp, "{0:fi\nnf nx}", arg); // DO NOT TRANSLATE
					ret = (ret.Length == 0) ? Properties.Resources.PersonNameUnknown : ret;
                    ret = base.CollapseWhitespace(ret);
                    break;
                case FMLast:
                    ret = string.Format(fp, "{0:gi nf nx}", arg); // DO NOT TRANSLATE
					ret = (ret.Length == 0) ? Properties.Resources.PersonNameUnknown : ret;
                    ret = base.CollapseWhitespace(ret);
                    break;
                case FMSlashLast:
                    ret = string.Format(fp, "{0:gi\nnf nx}", arg); // DO NOT TRANSLATE
					ret = (ret.Length == 0) ? Properties.Resources.PersonNameUnknown : ret;
                    ret = base.CollapseWhitespace(ret);
                    break;
                case Last:
                    ret = string.Format(fp, "{0:nf}", arg); // DO NOT TRANSLATE
					ret = (ret.Length == 0) ? Properties.Resources.PersonNameUnknown : ret;
                    ret = base.CollapseWhitespace(ret);
                    break;
                case FirstMiddle:
                    ret = string.Format(fp, "{0:ng nx}", arg);
					ret = (ret.Length == 0) ? Properties.Resources.PersonNameUnknown : ret;
                    ret = base.CollapseWhitespace(ret);
                    break;
                default:
                    ret = base.InnerFormat(format, arg, formatProvider);
                    break;
            }

            return ret.Trim(s_trim); // remove leading or trailing whitespace; remove trailing delimiter
        }


        protected override CustomFormatter GetCustomFormatter(object arg)
        {
            return new MyCustomFormatter((IPersonName)arg, _caps);
        }

        protected class MyCustomFormatter : CustomFormatter
        {
            protected IPersonName _name;
            protected bool _caps;

            public MyCustomFormatter(IPersonName name, bool caps)
            {
                _name = name;
                _caps = caps;
            }

            public override string ReplaceTerm(string term)
            {
                string ret = string.Empty;
                switch (term)
                {
                    case Family:
                    {
                        if (_name.FamilyName != null)
                        {
                            ret = ( _caps ) ? _name.FamilyName.ToUpper() : _name.FamilyName;
                        }
                        break;
                    }
                    case Given: ret = _name.GivenName; break;
                    case Suffix: ret = _name.NameSuffix; break;
                    case GivenAbbreviated: //First M.
                        {
                            ret = GivenAbbreviatedString(_name.GivenName); 
                            break;
                        }
                    case GivenFirst: //First
                        {
                            ret = GivenFirstString(_name.GivenName);
                            break;
                        }
                    case GivenInitials: //F.M.
                        {
                            ret = GivenInitialsString(_name.GivenName);
                            break;
                        }
                    case FirstInitial: //F.
                        {
                            ret = FirstInitialString(_name.GivenName);
                            break;
                        }

                    // Obsolete tokens
                    case "fc": ret = _name.FamilyName.ToUpper(); break;

                    default: ret = base.ReplaceTerm(term); break;
                }

                return ret;
            }
            
            //Returns a given name if the format of "F."
            private string FirstInitialString(string givenName)
            {
                if (givenName != string.Empty)
                {
                    return givenName.Substring(0, 1) + "."; // DO NOT TRANSLATE
                }
                else
                {
                    return string.Empty;
                }
            }

            //Returns a given name if the format of "F.M."
            private string GivenInitialsString(string givenName)
            {
                if (givenName != string.Empty)
                {
                    string name = string.Empty;
                    string[] names = givenName.Split(' ');
                    if (names != null && names.Length > 0)
                    {
                        foreach (string nameElement in names)
                        {
                            name = name + nameElement.Substring(0, 1) + "."; // DO NOT TRANSLATE
                        }

                        return name;
                    }
                    else
                    {
                        return givenName.Substring(0, 1) + "."; // DO NOT TRANSLATE
                    }
                }
                else
                {
                    return string.Empty;
                }
            }

            //Returns a given name if the format of "First"
            private string GivenFirstString(string givenName)
            {
                if (givenName != string.Empty)
                {
                    string[] names = givenName.Split(' ');
                    if (names != null && names.Length > 0)
                    {
                        return names[0];
                    }
                    else
                    {
                        return givenName;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }

            //Returns a given name if the format of "First M."
            private string GivenAbbreviatedString(string givenName)
            {
                if (givenName != string.Empty)
                {
                    string name = string.Empty;
                    string[] names = givenName.Split(' ');
                    if (names != null && names.Length > 0)
                    {
                        for (int i = 0; i < names.Length; i++)
                        {
                            if (i == 0)
                            {
                                name = name + names[0] + " "; // DO NOT TRANSLATE
                            }
                            else
                            {
                                if( !String.IsNullOrEmpty( names[i] ) )
                                {
                                    name = name + names[i].Substring( 0, 1 ) + "."; // DO NOT TRANSLATE
                                }
                            }
                        }
                        return name.Trim();
                    }
                    else
                    {
                        return givenName;
                    }
                }
                else
                {
                    return string.Empty;
                }

            }
        }
    }
}
