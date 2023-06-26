#pragma warning disable IDE0240
#nullable enable
#pragma warning restore IDE0240
using System;
using System.Collections.Generic;

namespace Get.EasyCSharp;

/// <summary>
/// Makes an overload without the parameter that calls the method with parameter
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true, Inherited = false)]
class OptionalParameterAttribute : MethodGeneratorAttribute
{
    /// <param name="ParameterName">Parameter Name</param>
    /// <param name="ParameterValue">Parameter Value. This can be either a constant, or an expression string</param>
#pragma warning disable IDE0060
    public OptionalParameterAttribute(string ParameterName, object ParameterValue) { }
#pragma warning restore IDE0060
}

/// <summary>
/// Makes an overload without the parameter that calls the method with parameter
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true, Inherited = false)]
class SubstitudeParameterAttribute : MethodGeneratorAttribute
{
    /// <param name="ParameterName">Parameter Name</param>
    /// <param name="ConvertExpression">Convert Expression to convert the value</param>
#pragma warning disable IDE0060
    public SubstitudeParameterAttribute(string ParameterName, Type NewType, string ConvertExpression) { }
#pragma warning restore IDE0060
    /// <summary>
    /// Overrides the old parameter name. If null, use the same parameter name
    /// </summary>
    public string? ParameterNameOverride { get; set; }
}
abstract class MethodGeneratorAttribute : Attribute
{

}