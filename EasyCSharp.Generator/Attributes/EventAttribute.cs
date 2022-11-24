#pragma warning disable IDE0240
#nullable enable
#pragma warning restore IDE0240
using System;
namespace EasyCSharp;

/// <summary>
/// Generates a method with the same argument types as the given delegate type
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
class EventAttribute : Attribute
{
    public EventAttribute(Type Type) { }

    public string? Name { get; set; }
    public bool AgressiveInline { get; set; } = true;
    public GeneratorVisibility Visibility { get; set; } = GeneratorVisibility.Default;
}