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
namespace EasyCSharp.Generator.Generator
{
    [CopySource("MethodGeneratorAttributes", typeof(MethodGeneratorAttribute))]
    [TypeToConst("OptionalParameterAttr", typeof(OptionalParameterAttribute))]
    [TypeToConst("SubstitudeParameterAttr", typeof(SubstitudeParameterAttribute))]
    [AddAttributeConverter(typeof(OptionalParameterAttribute), ParametersAsString = "\"sample\", \"sample\"")]
    [AddAttributeConverter(typeof(SubstitudeParameterAttribute), ParametersAsString = "\"sample\", default(System.Type), \"sample\"")]
    [Generator]
    partial class MethodGenerator : AttributeBaseGenerator<
        MethodGeneratorAttribute,
        MethodGenerator.IMethodGeneratorAttributeWarpper,
        BaseMethodDeclarationSyntax,
        IMethodSymbol
    >
    {
        public interface IMethodGeneratorAttributeWarpper { }
        partial class OptionalParameterAttributeWarpper : IMethodGeneratorAttributeWarpper { }
        partial class SubstitudeParameterAttributeWarpper : IMethodGeneratorAttributeWarpper { }
        static readonly string MethodImplAttr = typeof(System.Runtime.CompilerServices.MethodImplAttribute).FullName;
        static readonly string AggressiveInliningValue = $"{typeof(System.Runtime.CompilerServices.MethodImplOptions).FullName}.{nameof(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)}";
        
        protected override void OnInitialize(IncrementalGeneratorPostInitializationContext context)
        {
            context.AddSource($"EasyCSharp.MethodGeneratorAttributes.g.cs", MethodGeneratorAttributes);
        }

        protected override IMethodGeneratorAttributeWarpper? TransformAttribute(AttributeData attributeData, Compilation compilation)
            => attributeData.AttributeClass?.ToString() switch
            {
                OptionalParameterAttr => AttributeDataToOptionalParameterAttribute(attributeData, compilation),
                SubstitudeParameterAttr => AttributeDataToSubstitudeParameterAttribute(attributeData, compilation),
                _ => null
            };
        
        protected override string? OnPointVisit(GeneratorSyntaxContext genContext, BaseMethodDeclarationSyntax syntaxNode, IMethodSymbol symbol, (AttributeData Original, IMethodGeneratorAttributeWarpper Wrapper)[] attributeData)
        {
            return
                GetCode(symbol, attributeData, genContext.SemanticModel.Compilation)
                .JoinNewLine();
        }
        IEnumerable<string> GetCode(IMethodSymbol method, (AttributeData Original, IMethodGeneratorAttributeWarpper Wrapper)[] attributeDatas, Compilation compilation)
        {
            foreach (var attrs in attributeDatas.AllCombinations()) {
                if (attrs.Length == 0) continue; // We should not generate the original method.

                var visiblity = method.DeclaredAccessibility.ToString().ToLower();
                var @default = (OriginalName: default(string), default(ITypeSymbol), default(string), default(ITypeSymbol), default(string));
                var output =
                (
                    from x in method.Parameters
                    select
                        (
                            from attr in attrs
                            select attr.Wrapper switch
                            {
                                OptionalParameterAttributeWarpper op =>
                                    x.Name == op.ParameterName ?
                                    (OriginalName: x.Name, x.Type, default(string), default(ITypeSymbol), op.ParameterValue?.ToSyntaxString() ?? "null") :
                                    @default,
                                SubstitudeParameterAttributeWarpper sub =>
                                x.Name == sub.ParameterName ?
                                    (OriginalName: x.Name, x.Type, sub.ParameterNameOverride ?? x.Name, sub.NewType, sub.ConvertExpression.ToString()) :
                                    @default,
                                _ => throw new ArgumentException("Unreachable Exception")
                            }
                        )
                        .Where(x => x.OriginalName is not null)
                        .FirstOrDefault((x.Name, x.Type, x.Name, x.Type, x.Name))
                ).ToArray<(string OriginalName, ITypeSymbol OriginalType, string NewName, ITypeSymbol NewType, string Converter)>();

                var Front = $"{visiblity}{(method.IsStatic ? " static" : "")}";

                var ParameterHeader = string.Join(", ",
                    from x in output
                    where x.NewName is not null
                    select $"{x.NewType} {x.NewName}"
                );

                var CallExpression =
                    $"""
                    {(method.Name is ".ctor" ? "this" : method.Name)}(
                        {
                            (
                                from x in output
                                select $"{x.OriginalName}: {x.Converter},"
                            ).JoinNewLine().IndentWOF(1)
                        }
                    )
                    """;
                if (method.Name is ".ctor")
                    yield return $$"""
                    /// <summary>
                    /// <inheritdocs cref="{{method.ToDisplayString()}}" />
                    /// </summary>
                    {{Front}} {{method.ContainingType.Name}}({{ParameterHeader}}) :
                        {{CallExpression.IndentWOF(1)}}
                    { }
                    """;
                else
                    yield return $$"""
                    /// <summary>
                    /// <inheritdocs cref="{{method.ToDisplayString()}}" />
                    /// </summary>
                    {{Front}} {{method.ReturnType.FullName()}} {{method.Name}}({{ParameterHeader}}) {
                        {{CallExpression.IndentWOF(1)}};
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
