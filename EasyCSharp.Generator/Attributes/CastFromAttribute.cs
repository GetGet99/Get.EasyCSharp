using System;

namespace EasyCSharp;
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class CastFromAttribute : Attribute {
    public CastFromAttribute(Type Type) { }
}
