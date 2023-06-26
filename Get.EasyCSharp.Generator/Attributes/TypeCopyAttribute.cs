#pragma warning disable IDE0240
#nullable enable
#pragma warning restore IDE0240
using System;
using System.Collections.Generic;

namespace EasyCSharp;

/// <summary>
/// Makes an overload without the parameter that calls the method with parameter
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
class TypeCopyAttribute : MethodGeneratorAttribute
{
    /// <param name="Type">Type to copy from</param>
#pragma warning disable IDE0060
    public TypeCopyAttribute(Type Type) { }
#pragma warning restore IDE0060
}