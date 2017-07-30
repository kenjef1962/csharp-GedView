using System;

namespace NameDateParsing
{
    public interface IRange<T> : IComparable<IRange<T>>
    {
        T Begin { get; }
        T End { get; }
    }
}
