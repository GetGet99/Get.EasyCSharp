using System;

namespace CopySourceGenerator
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TypeToConstAttribute : Attribute
    {
        public TypeToConstAttribute(string MemberName, Type Type)
        {

        }
    }
}