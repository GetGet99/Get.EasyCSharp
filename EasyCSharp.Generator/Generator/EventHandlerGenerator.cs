using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Text;
using EasyCSharp.GeneratorTools;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CopySourceGenerator;
using static EasyCSharp.Generator.Generator.MethodGenerator;

namespace EasyCSharp.Generator.Generator
{
    [CopySource("EventSource", typeof(EventAttribute))]
    [CopySource("CastFromSource", typeof(CastFromAttribute))]
    [AddAttributeConverter(typeof(EventAttribute), ParametersAsString = "default(System.Type)")]
    [AddAttributeConverter(typeof(CastFromAttribute), ParametersAsString = "default(System.Type)")]
    [AddAttributeConverter(typeof(CastAttribute))]
    [Generator]
    internal partial class EventHandlerGenerator : AttributeBaseGenerator<
        EventAttribute,
        EventHandlerGenerator.EventAttributeWarpper,
        MethodDeclarationSyntax,
        IMethodSymbol
    >
    {
        static readonly string CastAttr = typeof(CastAttribute).FullName;
        static readonly string CastFromAttr = typeof(CastFromAttribute).FullName;
        static readonly string MethodImplAttr = typeof(System.Runtime.CompilerServices.MethodImplAttribute).FullName;
        static readonly string ArgumentException = typeof(ArgumentException).FullName;
        static readonly string AggressiveInliningValue = $"{typeof(System.Runtime.CompilerServices.MethodImplOptions).FullName}.{nameof(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)}";
        
        protected override void OnInitialize(IncrementalGeneratorPostInitializationContext context)
        {
            context.AddSource($"{typeof(EventAttribute).FullName}.g.cs", EventSource);
            context.AddSource($"{typeof(CastFromAttribute).FullName}.g.cs", CastFromSource);
        }
        
        protected override EventAttributeWarpper TransformAttribute(AttributeData attributeData, Compilation compilation)
            => AttributeDataToEventAttribute(attributeData, compilation);
        
        protected override string? OnPointVisit(GeneratorSyntaxContext genContext, MethodDeclarationSyntax syntaxNode, IMethodSymbol symbol, (AttributeData Original, EventAttributeWarpper Wrapper)[] attributeData)
        {
            return GetCode(symbol, attributeData, genContext.SemanticModel.Compilation).JoinNewLine();
        }
        IEnumerable<string> GetCode(IMethodSymbol method, (AttributeData Original, EventAttributeWarpper Wrapper)[] attributeDatas, Compilation compilation)
        {
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

                var delegateMethod = attr.Type.DelegateInvokeMethod;
                if (delegateMethod is null)
                {
                    // Error
                    yield return $"// Error: {attr.Type} is not a Delegate Type.";
                    continue;
                }

                var paramsWithCast =
                (
                    from y in method.Parameters.Enumerate()
                    let castAttr = y.Item.GetAttributes()
                    .SingleOrDefault(x =>
                        x.AttributeClass?.ToDisplayString() == CastFromAttr ||
                        x.AttributeClass?.ToDisplayString() == CastAttr
                    )
                    select (
                        Index: y.Index,
                        methodParam: y.Item,
                        castFromType: castAttr is null ? default :
                        
                        (
                            castAttr.AttributeClass?.ToDisplayString() == CastFromAttr ?
                            AttributeDataToCastFromAttribute(castAttr, compilation).Type :
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
                    let type = x.Item.MatchedParam.castFromType ?? x.Item.Param.Type
                    select $"{(
                                // Type
                                x.Item.MatchedParam.methodParam is null ?
                                type.WithNullableAnnotation(NullableAnnotation.Annotated).FullName(true) :
                                type.FullName(type.NullableAnnotation == NullableAnnotation.Annotated)
                            )} {(
                                // Name
                                x.Item.MatchedParam.methodParam is null ?
                                $"__{x.Index + 1}" :
                                x.Item.MatchedParam.methodParam.Name
                            )}"
                );
                var CallParameters = string.Join(", ",
                    from x in paramsWithCast
                    select (
                        x.cast ? (
                            x.methodParam.Type.NullableAnnotation is NullableAnnotation.Annotated ?
                            $"({x.methodParam.Type.FullName()}){x.methodParam.Name}" :
                            $"({x.methodParam.Name} as {x.methodParam.Type.FullName()} ?? " +
                            $$"""
                            throw new {{ArgumentException}}($"The parameter {nameof({{x.methodParam.Name}})} must be of type '{{x.methodParam.Type.FullName()}}' and is not null", nameof({{x.methodParam.Name}})))
                            """
                        ) : x.methodParam.Name
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
}
