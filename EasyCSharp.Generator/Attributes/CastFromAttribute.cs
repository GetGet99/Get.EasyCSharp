using System;

namespace EasyCSharp;
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
class CastFromAttribute : Attribute {
    public CastFromAttribute(Type Type) { }
}
