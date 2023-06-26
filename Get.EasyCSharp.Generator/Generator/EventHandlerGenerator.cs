using System;
using System.Collections.Generic;
using System.Linq;
using Get.EasyCSharp.GeneratorTools;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CopySourceGenerator;
using System.Diagnostics;

namespace Get.EasyCSharp.Generator;

[CopySource("EventSource", typeof(EventAttributeBase))]
[CopySource("CastFromSource", typeof(CastFromAttribute))]
[AddAttributeConverter(typeof(EventAttributeBase), SampleObjectType = typeof(EventAttribute), ParametersAsString = "default(System.Type)")]
[AddAttributeConverter(typeof(EventAttribute), ParametersAsString = "default(System.Type)")]
[AddAttributeConverter(typeof(EventAttribute<Action>), StructName = "EventAttributeGenericWrapper", MethodName = "AttributeDataToEventAttributeGeneric")]
[AddAttributeConverter(typeof(CastFromAttribute), ParametersAsString = "default(System.Type)")]
[AddAttributeConverter(typeof(CastFromAttribute<Action>),
    StructName = $"CastFromAttributeGenericWrapper",
    MethodName = "AttributeDataToCastFromAttributeGeneric"
)]
[AddAttributeConverter(typeof(CastAttribute))]
[Generator]
partial class EventHandlerGenerator : AttributeBaseGenerator<
    EventAttributeBase,
    EventHandlerGenerator.EventAttributeBaseWarpper,
    MethodDeclarationSyntax,
    IMethodSymbol
>
{
    static readonly string CastAttr = typeof(CastAttribute).FullName;
    static readonly string CastFromAttr = typeof(CastFromAttributeBase).FullName;
    static readonly string MethodImplAttr = typeof(System.Runtime.CompilerServices.MethodImplAttribute).FullName;
    static readonly string ArgumentException = typeof(ArgumentException).FullName;
    static readonly string AggressiveInliningValue = $"{typeof(System.Runtime.CompilerServices.MethodImplOptions).FullName}.{nameof(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)}";
    
    protected override void OnInitialize(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource($"{typeof(EventAttribute).FullName}.g.cs", EventSource);
        context.AddSource($"{typeof(CastFromAttributeBase).FullName}.g.cs", CastFromSource);
    }
    partial class EventAttributeBaseWarpper
    {
        public ITypeSymbol EventType { get; set; }
    }
    protected override EventAttributeBaseWarpper TransformAttribute(AttributeData attributeData, Compilation compilation)
    {
        var a = AttributeDataToEventAttributeBase(attributeData, compilation);
        if (attributeData.AttributeClass?.TypeArguments.Length > 0)
        {
            a.EventType = AttributeDataToEventAttributeGeneric(attributeData, compilation).DelegateType;
        } else
        {
            a.EventType = AttributeDataToEventAttribute(attributeData, compilation).Type;
        }
        return a;
    }
    
    protected override string? OnPointVisit(GeneratorSyntaxContext genContext, MethodDeclarationSyntax syntaxNode, IMethodSymbol symbol, (AttributeData Original, EventAttributeBaseWarpper Wrapper)[] attributeData)
    {
        return GetCode(symbol, attributeData, genContext.SemanticModel.Compilation).JoinNewLine();
    }
    IEnumerable<string> GetCode(IMethodSymbol method, (AttributeData Original, EventAttributeBaseWarpper Wrapper)[] attributeDatas, Compilation compilation)
    {
        var castFromBaseClass = compilation.GetTypeByMetadataName(CastFromAttr);
        foreach (var (_, attr) in attributeDatas) {
            var visiblity = GetVisiblityPrefix(
                method.DeclaredAccessibility.ToString().ToLower(),
                attr.Visibility
            );

            if (visiblity is null)
            {
                // Do not Generate
                yield return "// Visiblity is set to DoNotGenerate";
                continue;
            }

            var delegateMethod = (attr.EventType as INamedTypeSymbol)?.DelegateInvokeMethod;
            //if (!Debugger.IsAttached)Debugger.Launch();
            
            if (delegateMethod is null)
            {
                // Error
                yield return $"// Error: {attr.EventType} is not a Delegate Type.";
                continue;
            }

            var paramsWithCast =
            (
                from y in method.Parameters.Enumerate()
                let castAttr = y.Item.GetAttributes()
                .SingleOrDefault(x =>
                    (x.AttributeClass?.IsSubclassFrom(castFromBaseClass) ?? false) ||
                    x.AttributeClass?.ToDisplayString() == CastAttr
                )
                select (
                    Index: y.Index,
                    methodParam: y.Item,
                    castFromType: castAttr is null ? default :
                    
                    (
                        castAttr.AttributeClass?.IsSubclassFrom(castFromBaseClass) ?? false ?
                        (
                            castAttr.AttributeClass.TypeArguments.Length > 0 ?
                            AttributeDataToCastFromAttributeGeneric(castAttr, compilation).FromType :
                            AttributeDataToCastFromAttribute(castAttr, compilation).Type
                        ):
                        default
                    ),
                    cast: castAttr is not null
                )
            ).ToArray(); // Evaluate because we use it multiple times

            var annotatedParams =
            (
                from y in delegateMethod.Parameters.Enumerate()
                let MatchedParam =
                    paramsWithCast.FirstOrDefault(
                        x =>
                        (x.castFromType ?? x.methodParam.Type).Equals(y.Item.Type, SymbolEqualityComparer.Default) ||
                        (x.cast && x.castFromType is null && x.Index == y.Index)
                    )
                select (Param: y.Item, MatchedParam)
            ).ToArray();

            var AgressiveInline =
                attr.AgressiveInline ?
                // Agressive Inline is on
                $"[{MethodImplAttr}({AggressiveInliningValue})]" :
                // Agressive Inline is off
                "// AgressiveInline is false";

            var Front = $"{visiblity}{(attr.ForceStatic || method.IsStatic ? " static" : "")}";
            
            var EventName = attr.Name ?? method.Name;
            
            var EventParameters = string.Join(", ",
                from x in annotatedParams.Enumerate()
                let type = (x.Item.MatchedParam.castFromType ?? x.Item.Param.Type).WithNullableAnnotation(x.Item.Param.NullableAnnotation)
                select $"{(
                            // Type
                            x.Item.MatchedParam.methodParam is null ?
                            type.WithNullableAnnotation(NullableAnnotation.Annotated).FullName(true) :
                            type.FullName(type.NullableAnnotation == NullableAnnotation.Annotated)
                        )} {(
                            // Name
                            x.Item.MatchedParam.methodParam is null ?
                            $"__{x.Index + 1}" :
                            x.Item.MatchedParam.methodParam.GetEscapedName()
                        )}"
            );
            var CallParameters = string.Join(", ",
                from x in paramsWithCast
                select (
                    x.cast ? (
                        x.methodParam.Type.NullableAnnotation is NullableAnnotation.Annotated ?
                        $"({x.methodParam.Type.FullName()}){x.methodParam.GetEscapedName()}" :
                        $"({x.methodParam.GetEscapedName()} as {x.methodParam.Type.FullName()} ?? " +
                        $$"""
                        throw new {{ArgumentException}}($"The parameter {nameof({{x.methodParam.GetEscapedName()}})} must be of type '{{x.methodParam.Type.FullName()}}' and is not null", nameof({{x.methodParam.Name}})))
                        """
                    ) : x.methodParam.GetEscapedName()
                )
            );
            // Generate
            yield return
                $$"""
                /// <summary>
                /// <inheritdoc cref="{{method.ToDisplayString()}}"/>
                /// </summary>
                {{AgressiveInline}}
                {{Front}} {{method.ReturnType.FullName()}} {{EventName}}({{EventParameters}}) {
                    {{method.Name}}({{CallParameters}});
                }
                """;
        }
    }
    static string? GetVisiblityPrefix(string DefaultPrefix, GeneratorVisibility propertyVisibility)
        => propertyVisibility switch
        {
            GeneratorVisibility.Default => DefaultPrefix,
            GeneratorVisibility.DoNotGenerate => null,
            GeneratorVisibility.Public => $"public",
            GeneratorVisibility.Private => $"private",
            GeneratorVisibility.Protected => $"protected",
            _ => throw new ArgumentOutOfRangeException()
        };
}
