using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EasyCSharp.CustomGenerator
{
    public class CustomGeneratorAttribute
    {

    }
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class BaseCustomGeneratorAttribute : Attribute
    {
        protected abstract Type GeneratorClass { get; }
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EnsureExtend : Attribute
    {
        public EnsureExtend(params Type[] Types)
        {

        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class EnsureMember : Attribute
    {
        public EnsureMember(string MemberName, string DeclareSyntaxIfNotFound)
        {
            
        }
    }
    public abstract class CustomGenerator<T> : CustomGeneratorBase where T : BaseCustomGeneratorAttribute
    {
        protected abstract string OnGeneratorVisitField(IFieldInfo<T> fieldInfo);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class CustomGeneratorBase
    {

    }
    enum TypeMode
    {
        Struct,
        Class
    }
    public interface ITypeHeaderInfo
    {
        string Namespace { get; }
        string Type { get; }
        string Name { get; }
    }
    public interface IFieldInfo<T>
    {
        T Attribute { get; }
        string ContainingType { get; }
        string Type { get; }
        string Name { get; }
        string GetSuggestedPropertyName(string? UserDefinedPropertyName);
    }
    public interface IMethodInfo
    {
        string Type { get; }
        string Name { get; }
        IEnumerable<IParameterInfo> Parameters { get; }
    }
    public interface IParameterInfo
    {
        string Type { get; }
        string Name { get; }
    }
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
    }
}
