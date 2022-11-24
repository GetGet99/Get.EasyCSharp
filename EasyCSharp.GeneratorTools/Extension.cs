using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCSharp.GeneratorTools
{
    static class Extension
    {
        public static string Indent(this string Original, int IndentTimes = 1, int IndentSpace = 4)
        {
            var Indent = new string(' ', IndentSpace * IndentTimes);
            var slashNindent = $"\n{Indent}";
            return Indent + Original.Replace("\n", slashNindent);
        }
        public static string IndentWOF(this string Original, int IndentTimes = 1, int IndentSpace = 4)
        {
            var Indent = new string(' ', IndentSpace * IndentTimes);
            var slashNindent = $"\n{Indent}";
            return Original.Replace("\n", slashNindent);
        }
        public static IEnumerable<(uint Index, T Item)> Enumerate<T>(this IEnumerable<T> TEnu)
        {
            uint i = 0;
            foreach (var a in TEnu)
                yield return (i++, a);
        }
        public static T CastOrDefault<T>(this object? Value, T DefaultValue)
        {
            if (Value is T CastedValue) return CastedValue;
            else return DefaultValue;
        }
    }
}
