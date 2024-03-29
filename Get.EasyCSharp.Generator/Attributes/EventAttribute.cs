﻿#pragma warning disable IDE0240
#nullable enable
#pragma warning restore IDE0240
using System;
namespace Get.EasyCSharp;

/// <summary>
/// Please do not use this class
/// </summary>
abstract class EventAttributeBase : Attribute
{
    /// <summary>
    /// The name of the event, <c>null</c> defaults to the same name as the method
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// Whether to requet the compiler to inline the method if possible or not
    /// </summary>
    public bool AgressiveInline { get; set; } = true;
    /// <summary>
    /// Force the generated event to be static.
    /// If <see cref="ForceStatic"/> is false,
    /// it follows the defined method. Otherwise,
    /// always generate the event in static mode.
    /// </summary>
    public bool ForceStatic { get; set; } = false;
    /// <summary>
    /// Sets the visibility of the event.
    /// Default means the same visiblity as the method.
    /// </summary>
    public GeneratorVisibility Visibility { get; set; } = GeneratorVisibility.Default;
}

/// <summary>
/// Generates a method with the same argument types as the given delegate type
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
class EventAttribute : EventAttributeBase
{
    /// <param name="Type">Event Type, must be a <see cref="Delegate"/></param>
#pragma warning disable IDE0060
    public EventAttribute(Type Type) { }
#pragma warning restore IDE0060
}

/// <summary>
/// Generates a method with the same argument types as the given delegate type
/// </summary>
/// <typeparam name="DelegateType">Event Type, must be a <see cref="Delegate"/></typeparam>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
class EventAttribute<DelegateType> : EventAttributeBase where DelegateType : Delegate
{
    public EventAttribute() { }
}