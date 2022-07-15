namespace AnimDL.Helpers;

public static class RangeExtensions
{
    public static IAsyncEnumerable<T> Slice<T>(this IAsyncEnumerable<T> enumerable, Range range)
    {
        if (range.Start.Value == 0 && range.End.Value == 0)
        {
            return enumerable;
        }

        var result = enumerable;
        var count = Math.Abs(range.End.Value - range.Start.Value) + 1;

        result = range.Start.IsFromEnd ? result.TakeLast(range.Start.Value) : result.Skip(range.Start.Value - 1);

        return result.Take(count);
    }
}
