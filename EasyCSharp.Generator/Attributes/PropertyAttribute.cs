using System;
using System.Diagnostics;
namespace EasyCSharp;

enum PropertyVisibility : byte { Default = default, Public, Protected, Private, DoNotGenerate }

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
[Conditional("AutoNotifyGenerator_DEBUG")]
class PropertyAttribute : Attribute
{
    public PropertyAttribute()
    {
    }
    public string? PropertyName { get; set; }
    public string? OnChanged { get; set; }
    public bool AgressiveInline { get; set; } = true;
    public bool OverrideKeyword { get; set; } = false;
    public PropertyVisibility Visibility { get; set; } = PropertyVisibility.Public;
    public PropertyVisibility GetVisibility { get; set; } = PropertyVisibility.Default;
    public PropertyVisibility SetVisibility { get; set; } = PropertyVisibility.Default;
}