using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NameDateParsing
{
    /// <summary>
    /// Base class for all formatting objects
    /// </summary>
    public abstract class FormatInfo : ICustomFormatterEx, IFormatProvider, ICloneable
    {
        protected string defaultFormat = General;
        private static Regex s_collapse = new Regex(@"\s\s+", RegexOptions.Compiled); // DO NOT TRANSLATE
        private static Regex s_terms = new Regex(@"\w+", RegexOptions.Compiled); // DO NOT TRANSLATE
        private static Dictionary<Type, FormatInfo> s_formaterMap;

        static FormatInfo()
        {
            s_formaterMap = new Dictionary<Type, FormatInfo>();
        }

        protected FormatInfo()
        {
            Initialize(General);
        }

        protected FormatInfo(string defaultFormat)
        {
            Initialize(defaultFormat);
        }

        public static T GetDefault<I, T>() where T : FormatInfo
        {
            if (!s_formaterMap.ContainsKey(typeof(I)))
            {
                s_formaterMap[typeof(I)] = Activator.CreateInstance<T>();
            }

            return s_formaterMap[typeof(I)] as T;
        }

        public static FormatInfo SetDefault<I, T>(FormatInfo value) where T : FormatInfo
        {
            FormatInfo oldValue = null;
            s_formaterMap.TryGetValue(typeof(I), out oldValue);
            System.Diagnostics.Debug.Assert(value is T);
            s_formaterMap[typeof(I)] = value;

            return oldValue;
        }

        private void Initialize(string defaultFormat)
        {
            this.defaultFormat = defaultFormat;
        }

        protected virtual string GetCacheKey(object obj)
        {
            return null;
        }

        /// <summary>
        /// General or standard format
        /// </summary>
        public const string General = "G"; // DO NOT TRANSLATE

        #region ICustomFormatter Members

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null) throw new ArgumentNullException("arg"); // DO NOT TRANSLATE
            IFormatProvider fp = (formatProvider == null) ? this : formatProvider;

            if (format == null)
            {
                if (fp is ICustomFormatterEx)
                {
                    format = (fp as ICustomFormatterEx).DefaultFormat;
                }
                else
                {
                    format = General;
                }
            }

			return _InnerFormat(format, arg, formatProvider);
        }

        #endregion

        #region ICustomFormatterEx Members

        public virtual string DefaultFormat
        {
            get { return defaultFormat; }
            set { defaultFormat = value; }
        }

        #endregion

        #region IFormatProvider Members

        /// <summary>
        /// Gets an object that provides formatting services for the specified type.
        /// </summary>
        /// <param name="formatType">An object that specifies the type of format object to get.</param>
        /// <returns>
        /// The current instance, if formatType is the same type as the current instance; otherwise, null.
        /// </returns>
        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
            {
                return this;
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Formats the object using the specified custom formatting string
        /// </summary>
        /// <param name="format">The custom formatting string</param>
        /// <param name="arg">The object to be formatted</param>
        /// <returns>Custom formatted string</returns>
        protected virtual string FormatCustom(string format, object arg)
        {
            return s_terms.Replace(format, new MatchEvaluator(GetCustomFormatter(arg).ReplaceTerm));
        }

        #region CustomFormatter

        protected virtual CustomFormatter GetCustomFormatter(object arg)
        {
            return new CustomFormatter();
        }

        protected class CustomFormatter
        {
            public virtual string ReplaceTerm(string term)
            {
                return term;
            }
            
            internal string ReplaceTerm(Match m)
            {
                return ReplaceTerm(m.Value);
            }
        }

        #endregion

        /// <summary>
        /// remove duplicate whitespace characters
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        protected string CollapseWhitespace(string s)
        {
            return s_collapse.Replace(s, new MatchEvaluator(WhitespaceReplace)).Trim();
        }

        private string WhitespaceReplace(Match m)
        {
            if (m.Value.Contains("\n")) return "\n"; // DO NOT TRANSLATE
            else return " "; // DO NOT TRANSLATE
        }

        protected virtual string InnerFormat(string format, object arg, IFormatProvider formatProvider)
        {
            if (formatProvider is FormatInfo)
            {
                return (formatProvider as FormatInfo).FormatCustom(format, arg);
            }
            else
            {
                return FormatCustom(format, arg);
            }
        }

        /// <summary>
        /// Allows derived class to do any cleanup on the resulting string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        protected virtual string PostFormat(string s)
        {
            return s;
        }

        private string _InnerFormat(string format, object obj, IFormatProvider formatProvider)
        {
            string ret = string.Empty;

            object formatter = (formatProvider == null) ? null : formatProvider.GetFormat(typeof(ICustomFormatter));
            if (null == formatter)
            {
                 ret = InnerFormat(format, obj, formatProvider);
            }
            else
            {
                ret = (formatter as ICustomFormatter).Format(format, obj, null);
            }

            return PostFormat(ret);
        }

        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
