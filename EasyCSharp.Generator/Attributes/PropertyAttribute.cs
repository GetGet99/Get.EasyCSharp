#pragma warning disable IDE0240
#nullable enable
#pragma warning restore IDE0240
using System;
using System.Diagnostics;
namespace EasyCSharp;

enum GeneratorVisibility : byte { Default = default, Public, Protected, Private, DoNotGenerate }

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
class PropertyAttribute : Attribute
{
    /// <summary>
    /// Overriding the generated Property Name
    /// </summary>
    public string? PropertyName { get; set; }
    /// <summary>
    /// Custom Type, Default to the same type applied to the field.
    /// Make sure to set <see cref="CustomGetExpression"/> and <see cref="CustomSetExpression"/> to handle the new property type.
    /// </summary>
    public Type? CustomType { get; set; }
    /// <summary>
    /// Custom Set Expression, Default to the field name
    /// </summary>
    public string? CustomGetExpression { get; set; }
    /// <summary>
    /// Custom Set Expression, Default to "value"
    /// </summary>
    public string? CustomSetExpression { get; set; }
    /// <summary>
    /// The function to call when set is called after the new value is set
    /// </summary>
    public string? OnChanged { get; set; }
    /// <summary>
    /// The function to call when set is called before the new value is set
    /// </summary>
    public string? OnBeforeChanged { get; set; }
    /// <summary>
    /// Set whether compiler should Inline the property call
    /// </summary>
    public bool AgressiveInline { get; set; } = true;
    /// <summary>
    /// Should "override" keyword be added
    /// </summary>
    public bool OverrideKeyword { get; set; } = false;
    /// <summary>
    /// Visiblity of the property, setting to <see cref="GeneratorVisibility.DoNotGenerate"/> will not generate the method, making this attribute useless
    /// </summary>
    public GeneratorVisibility Visibility { get; set; } = GeneratorVisibility.Public;
    /// <summary>
    /// Visibility of "get" part, default means do not add the visiblity
    /// </summary>
    public GeneratorVisibility GetVisibility { get; set; } = GeneratorVisibility.Default;
    /// <summary>
    /// Visibility of "set" part, default means do not add the visiblity
    /// </summary>
    public GeneratorVisibility SetVisibility { get; set; } = GeneratorVisibility.Default;
}