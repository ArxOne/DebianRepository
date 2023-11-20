using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ArxOne.Debian.Utility;

public static class ArrayExtensions
{
    public static void Deconstruct<T>(this IEnumerable<T> items, out T t0, out T t1, out T t2, out T t3)
    {
        if (items is null)
            throw new ArgumentException(nameof(items));
        var array = items.ToImmutableArray();
        if (array.Length < 4)
            throw new ArgumentException(nameof(items));

        t0 = array[0];
        t1 = array[1];
        t2 = array[2];
        t3 = array[3];
    }
}
