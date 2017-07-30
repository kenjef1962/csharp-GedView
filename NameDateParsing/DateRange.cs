using System;

namespace NameDateParsing
{
    public class DateRange : SDNDate, IRange<Date>, IRange<IDate>, IEquatable<DateRange>
    {
        SDNDate _end;

        internal DateRange(uint begin, uint end)
            : base(begin)
        {
            _end = new SDNDate(end);
        }

        public SDNDate Begin
        {
            get { return new SDNDate((uint)this._code); }
        }

        public SDNDate End
        {
            get { return _end; }
        }

        #region IRange<Date> Members

        Date IRange<Date>.Begin
        {
            get { return Begin; }
        }

        Date IRange<Date>.End
        {
            get { return End; }
        }

        #endregion

        #region IRange<IDate> Members

        IDate IRange<IDate>.Begin
        {
            get { return Begin; }
        }

        IDate IRange<IDate>.End
        {
            get { return End; }
        }

        #endregion

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ _end.GetHashCode();
        }


        #region IEquatable<DateRange> Members

        bool IEquatable<DateRange>.Equals(DateRange other)
        {
            return (this as IDate).Equals(other.Begin) && _end.Equals(other.End);
        }

        #endregion

        #region IComparable<IRange<IDate>> Members

        int IComparable<IRange<IDate>>.CompareTo(IRange<IDate> other)
        {
            return (this as IComparable<IRange<Date>>).CompareTo((IRange<Date>)other);
        }

        #endregion

        #region IComparable<IRange<Date>> Members

        int IComparable<IRange<Date>>.CompareTo(IRange<Date> other)
        {
            int result = (this as IComparable<Date>).CompareTo(other.Begin);

            if (result == 0)
            {
                result = (_end as IComparable<Date>).CompareTo(other.End);
            }

            return result;
        }

        #endregion

    }


}
