﻿namespace Get.EasyCSharp;

public static class RangeExtension
{
    /// <summary>
    /// Create Enumerator from Range, for `foreach` syntax sugar
    /// </summary>
    /// <param name="range">The `range` to create the enumerable. Indexing from end infers ending from length</param>
    /// <returns>Enumerable containing all sequence from start to end</returns>
    public static IEnumerator<int> GetEnumerator(this Range range)
    {
        foreach (var a in GetEnumerable(range)) yield return a;
    }
    /// <summary>
    /// Create Enumerator from Range, for `foreach` syntax sugar
    /// </summary>
    /// <param name="range">The `range` to create the enumerable. Indexing from end infers ending from length</param>
    /// <returns>Enumerable containing all sequence from start to end</returns>
    public static IEnumerable<TOut> Select<TOut>(this Range range, Func<int, TOut> func)
    {
        if (range.End.IsFromEnd) throw new ArgumentException("Range.End cannot start from the end value");
        if (range.Start.IsFromEnd) range = (range.End.Value - range.Start.Value)..range.End.Value;
        for (int i = range.Start.Value; i < range.End.Value; i++)
        {
            yield return func(i);
        }
    }
    /// <summary>
    /// Create Enumerable from Range, with given length
    /// </summary>
    /// <param name="range">The `range` to create the enumerable. Indexing from end infers ending from length</param>
    /// <param name="step">The step, defaults to 1</param>
    /// <param name="length">The length to refer to. If null, it refers to the end range</param>
    /// <returns>Enumerable containing all sequence from start to end</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static IEnumerable<int> GetEnumerable(Range range, int? length = null, int step = 1)
    {
        if (length is null)
        {
            length = range.End.Value;
            if (range.End.IsFromEnd)
                throw new ArgumentException("Range.End cannot start from the end value");
        }
        var (offset, len) = range.GetOffsetAndLength(length.Value);
        switch (step) {
            case > 0:
                for (int i = 0; i < len; i += step)
                {
                    yield return i + offset;
                }
                break;
            case < 0:
                for (int i = len - 1; i >= 0; i += step) // step is negative
                {
                    yield return i + offset;
                }
                break;
            default:
                throw new ArgumentException("step cannot be 0");
        }
    }
    /// <summary>
    /// Create Enumerable from Range
    /// </summary>
    /// <param name="range">The `range` to create the enumerable. `range.End` must not be from end. If `range.Start` is from end, the end value is infered from `range.End`</param>
    /// <returns>Enumerable containing all sequence from start to end</returns>
    /// <exception cref="ArgumentException"></exception>
    public static IEnumerable<int> GetEnumerable(Range range)
    {
        if (range.End.IsFromEnd) throw new ArgumentException("Range.End cannot start from the end value");
        if (range.Start.IsFromEnd) range = (range.End.Value-range.Start.Value)..range.End.Value;
        for (int i = range.Start.Value; i < range.End.Value; i++)
        {
            yield return i;
        }
    }

}
