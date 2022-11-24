#pragma warning disable IDE0240
#nullable enable
#pragma warning restore IDE0240

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace EasyCSharp.GeneratorTools
{
    static class Extension
    {
        public static string Indent(this string Original, int IndentTimes = 1, int IndentSpace = 4)
        {
            var Indent = new string(' ', IndentSpace * IndentTimes);
            var slashNindent = $"\r\n{Indent}";
            return Indent + Original.Replace("\r\n", slashNindent);
        }
        public static string JoinNewLine(this IEnumerable<string> Original)
            => string.Join("\r\n", Original);
        public static string JoinDoubleNewLine(this IEnumerable<string> Original)
            => string.Join("\r\n\r\n", Original);
        public static string IndentWOF(this string Original, int IndentTimes = 1, int IndentSpace = 4)
        {
            var Indent = new string(' ', IndentSpace * IndentTimes);
            var slashNindent = $"\r\n{Indent}";
            return Original.Replace("\r\n", slashNindent);
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
        public static T GetProperty<T>(this AttributeData attribute, string AttributeName, T defaultValue)
        {
            bool first = true;
            T result = defaultValue;
            foreach (var v in attribute.NamedArguments)
            {
                if (v.Key == AttributeName)
                {
                    if (!first)
                    {
                        throw new ArgumentException("Duplicate Element!");
                    }

                    first = false;
                    result = (T)v.Value!.Value!;
                }
            }
            return result;
        }
        public static IEnumerable<ISymbol> GetMemeberRecursiveBaseType(this ITypeSymbol? Type)
        {
            if (Type is null) yield break;

            foreach (var member in Type.GetMembers())
                yield return member;
            foreach (var member in Type.BaseType.GetMemeberRecursiveBaseType())
                yield return member;
        }
        //public static T GetConstructor<T>(this AttributeData attribute, int index, T? defaultValue = default)
        //{
        //var attr = attribute.
        //}
    }
}
