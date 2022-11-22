using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCSharp;
/// <summary>
/// Generates a method with the same argument types as the given delegate type
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class EventAttribute : Attribute
{
    public EventAttribute(Type Type) { }

    public string? Name { get; set; }
    public bool AgressiveInline { get; set; } = true;
    public PropertyVisibility Visibility { get; set; } = PropertyVisibility.Default;
}
