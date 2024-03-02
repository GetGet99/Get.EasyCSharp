using System;
using System.Collections.Generic;

namespace EasyCSharp.GeneratorTools.SyntaxCreator;

static partial class Extension
{
    public static string? GetString(this SyntaxVisibility visibility)
        => visibility switch
        {
            SyntaxVisibility.Default => null,
            SyntaxVisibility.Public => "public",
            SyntaxVisibility.Protected => "protected",
            SyntaxVisibility.Private => "private",
            // Incluing DoNotGenerate
            _ => throw new ArgumentException()
        };
    public static string JoinWith(this IEnumerable<string> enumerable, string separator)
        => string.Join(separator, enumerable);

    /// <summary>
    /// Allias helper for newer syntax
    /// </summary>
    /// <param name="values">Enumerable to add</param>
    public static void Add<T>(this ICollection<T> collection, IEnumerable<T> values)
    {
        foreach (var value in values)
            collection.Add(value);
    }
    /// <summary>
    /// Allias helper for newer syntax
    /// </summary>
    /// <param name="values">Enumerable to add</param>
    public static void Add<T>(this LinkedList<T> collection, T value)
        => collection.AddLast(value);
}
