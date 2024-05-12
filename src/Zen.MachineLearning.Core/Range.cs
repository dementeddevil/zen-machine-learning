using System.Numerics;

namespace Zen.MachineLearning.Core;

/// <summary>
/// Represents an integer range with minimum and maximum values.
/// </summary>
/// 
/// <remarks>
/// <para>The class represents an integer range with inclusive limits -
/// both minimum and maximum values of the range are included into it.
/// Mathematical notation of such range is <b>[min, max]</b>.</para>
/// 
/// <para>Sample usage:</para>
/// <code>
/// // create [1, 10] range
/// IntRange range1 = new IntRange( 1, 10 );
/// // create [5, 15] range
/// IntRange range2 = new IntRange( 5, 15 );
/// // check if values is inside of the first range
/// if ( range1.IsInside( 7 ) )
/// {
///     // ...
/// }
/// // check if the second range is inside of the first range
/// if ( range1.IsInside( range2 ) )
/// {
///     // ...
/// }
/// // check if two ranges overlap
/// if ( range1.IsOverlapping( range2 ) )
/// {
///     // ...
/// }
/// </code>
/// </remarks>
///
[Serializable]
public struct Range<T> where T : INumber<T>
{
    private T _min;
    private T _max;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntRange"/> structure.
    /// </summary>
    /// 
    /// <param name="min">Minimum value of the range.</param>
    /// <param name="max">Maximum value of the range.</param>
    /// 
    public Range(T min, T max)
    {
        _min = min;
        _max = max;
    }

    /// <summary>
    /// Minimum value of the range.
    /// </summary>
    /// 
    /// <remarks><para>The property represents minimum value (left side limit) or the range -
    /// [<b>min</b>, max].</para></remarks>
    /// 
    public T Min
    {
        get
        {
            return _min;
        }
        set
        {
            _min = value;
        }
    }

    /// <summary>
    /// Maximum value of the range.
    /// </summary>
    /// 
    /// <remarks><para>The property represents maximum value (right side limit) or the range -
    /// [min, <b>max</b>].</para></remarks>
    /// 
    public T Max
    {
        get
        {
            return _max;
        }
        set
        {
            _max = value;
        }
    }

    /// <summary>
    /// Length of the range (deffirence between maximum and minimum values).
    /// </summary>
    public T Length => _max - _min;

    /// <summary>
    /// Check if the specified value is inside of the range.
    /// </summary>
    /// 
    /// <param name="x">Value to check.</param>
    /// 
    /// <returns><b>True</b> if the specified value is inside of the range or
    /// <b>false</b> otherwise.</returns>
    /// 
    public bool IsInside(T x)
    {
        return x >= _min && x <= _max;
    }

    /// <summary>
    /// Check if the specified range is inside of the range.
    /// </summary>
    /// 
    /// <param name="range">Range to check.</param>
    /// 
    /// <returns><b>True</b> if the specified range is inside of the range or
    /// <b>false</b> otherwise.</returns>
    /// 
    public bool IsInside(Range<T> range)
    {
        return IsInside(range._min) && IsInside(range._max);
    }

    /// <summary>
    /// Check if the specified range overlaps with the range.
    /// </summary>
    /// 
    /// <param name="range">Range to check for overlapping.</param>
    /// 
    /// <returns><b>True</b> if the specified range overlaps with the range or
    /// <b>false</b> otherwise.</returns>
    /// 
    public bool IsOverlapping(Range<T> range)
    {
        return IsInside(range._min) || IsInside(range._max) ||
               range.IsInside(_min) || range.IsInside(_max);
    }

    /// <summary>
    /// Equality operator - checks if two ranges have equal min/max values.
    /// </summary>
    /// 
    /// <param name="range1">First range to check.</param>
    /// <param name="range2">Second range to check.</param>
    /// 
    /// <returns>Returns <see langword="true"/> if min/max values of specified
    /// ranges are equal.</returns>
    ///
    public static bool operator ==(Range<T> range1, Range<T> range2)
    {
        return range1._min == range2._min && range1._max == range2._max;
    }

    /// <summary>
    /// Inequality operator - checks if two ranges have different min/max values.
    /// </summary>
    /// 
    /// <param name="range1">First range to check.</param>
    /// <param name="range2">Second range to check.</param>
    /// 
    /// <returns>Returns <see langword="true"/> if min/max values of specified
    /// ranges are not equal.</returns>
    ///
    public static bool operator !=(Range<T> range1, Range<T> range2)
    {
        return range1._min != range2._min || range1._max != range2._max;

    }

    /// <summary>
    /// Check if this instance of <see cref="FloatRange"/> equal to the specified one.
    /// </summary>
    /// 
    /// <param name="obj">Another range to check equalty to.</param>
    /// 
    /// <returns>Return <see langword="true"/> if objects are equal.</returns>
    /// 
    public override bool Equals(object obj)
    {
        return obj is Range<T> ? this == (Range<T>)obj : false;
    }

    /// <summary>
    /// Get hash code for this instance.
    /// </summary>
    /// 
    /// <returns>Returns the hash code for this instance.</returns>
    /// 
    public override int GetHashCode()
    {
        return _min.GetHashCode() + _max.GetHashCode();
    }

    /// <summary>
    /// Get string representation of the class.
    /// </summary>
    /// 
    /// <returns>Returns string, which contains min/max values of the range in readable form.</returns>
    ///
    public override string ToString()
    {
        return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}, {1}", _min, _max);
    }
}
