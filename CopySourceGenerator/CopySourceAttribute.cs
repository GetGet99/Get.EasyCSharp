using System;

namespace CopySourceGenerator
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    class CopySourceAttribute : Attribute
    {
        public CopySourceAttribute(string MemberName, Type Type)
        {

        }
    }
}