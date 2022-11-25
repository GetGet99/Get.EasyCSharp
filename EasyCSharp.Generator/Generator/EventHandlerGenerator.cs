using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EasyCSharp.GeneratorTools;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using CopySourceGenerator;
namespace EasyCSharp;
[CopySource("EventSource", typeof(EventAttribute))]
[CopySource("CastFromSource", typeof(CastFromAttribute))]
[Generator]
public partial class EventHandlerGenerator : GeneratorBase<MethodAttributeSyntaxReceiver>
{
    protected override void OnInitialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(x =>
        {
            x.AddSource($"{typeof(EventAttribute).FullName}.g.cs", EventSource);
            x.AddSource($"{typeof(CastFromAttribute).FullName}.g.cs", CastFromSource);
        });
    }
    readonly static EventAttribute DefaultEventAttribute = new(typeof(EventHandler));
    protected override MethodAttributeSyntaxReceiver ConstructSyntaxReceiver()
        => new(typeof(EventAttribute).FullName);
    protected override void OnExecute(GeneratorExecutionContext context, MethodAttributeSyntaxReceiver SyntaxReceiver)
    {
        var CastFromAttributeSymbol = context.Compilation.GetTypeByMetadataName(typeof(CastFromAttribute).FullName);
        var EventAttributeSymbol = context.Compilation.GetTypeByMetadataName(typeof(EventAttribute).FullName);
        var TypeSymbol = context.Compilation.GetTypeByMetadataName(typeof(Type).FullName);
        foreach
            (
                var MethodSymbols in
                SyntaxReceiver.Methods
                .GroupBy<IMethodSymbol, INamedTypeSymbol>
                (f => f.ContainingType, SymbolEqualityComparer.Default)
            )
        {
            //Debugger.Launch();
            var ClassSymbol = MethodSymbols.Key;
            context.AddSource($"{ClassSymbol.Name}.GeneratedEvents.g.cs", $$"""
                    #nullable enable
                    using System.Runtime.CompilerServices;
                    namespace {{ClassSymbol.ContainingNamespace}}
                    {
                        partial class {{ClassSymbol.Name}}
                        {
                            {{(
                                from method in MethodSymbols
                                select (
                                    from attribute in method.GetAttributes()
                                    where attribute.AttributeClass?.Equals(EventAttributeSymbol, SymbolEqualityComparer.Default) ?? false
                                    let args = attribute.ConstructorArguments
                                    where args.Length == 1
                                    /// Constructor #1
                                    let arg = args[0]
                                    where arg.Type?.Equals(TypeSymbol, SymbolEqualityComparer.Default) ?? false
                                    let delegateInvoke = (arg.Value as INamedTypeSymbol)?.DelegateInvokeMethod
                                    where delegateInvoke is not null
                                    /// <see cref="EventAttribute.Name"/>
                                    let EventName = attribute.NamedArguments.SingleOrDefault(x => x.Key == nameof(EventAttribute.Name)).Value.Value.CastOrDefault(DefaultEventAttribute.Name)
                                    /// <see cref="EventAttribute.AgressiveInline"/>
                                    let Inline = attribute.NamedArguments.SingleOrDefault(x => x.Key == nameof(EventAttribute.AgressiveInline)).Value.Value.CastOrDefault(DefaultEventAttribute.AgressiveInline)
                                    /// <see cref="EventAttribute.Visibility"/>
                                    let VisibilityPrefix = GetVisibilityPrefix(
                                        method.DeclaredAccessibility.ToString().ToLower(),
                                        attribute.NamedArguments.SingleOrDefault(x => x.Key == nameof(EventAttribute.Visibility)).Value,
                                        DefaultEventAttribute.Visibility
                                    )
                                    // Visibility is not DoNotGenerate
                                    where VisibilityPrefix is not null

                                    let paramsWithCast =
                                    (
                                        from methodParam in method.Parameters
                                        let attr = methodParam.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Equals(CastFromAttributeSymbol, SymbolEqualityComparer.Default) ?? false)
                                        let castFromType = attr != null && attr.ConstructorArguments.Length > 0 ? attr.ConstructorArguments[0].Value as INamedTypeSymbol : null
                                        select (methodParam, castFromType)
                                    ).ToArray() // Evaluate because we use it multiple times

                                    let annotatedParams =
                                    (
                                        from Param in delegateInvoke.Parameters
                                        let MatchedParam =
                                            paramsWithCast.FirstOrDefault(
                                                x =>
                                                (x.castFromType ?? x.methodParam.Type).Equals(Param.Type, SymbolEqualityComparer.Default)
                                            )
                                        select (Param, MatchedParam)
                                    ).ToArray()
                                    select
                                        $$"""
                                            /// <summary>
                                            /// <inheritdoc cref="{{method.ToDisplayString()}}"/>
                                            /// </summary>
                                            {{(Inline ? "[MethodImpl(MethodImplOptions.AggressiveInlining)]" : "// Inline Disabled")}}
                                            {{VisibilityPrefix}}{{(method.IsStatic ? " static" : "")}} {{method.ReturnType}} {{EventName ?? method.Name}}({{string.Join(", ",
                                            from x in annotatedParams.Enumerate()
                                            let type = (x.Item.MatchedParam.castFromType ?? x.Item.Param.Type)
                                            select $"{(x.Item.MatchedParam.methodParam is null ?
                                                type.WithNullableAnnotation(NullableAnnotation.Annotated).ToDisplayString() :
                                                type.ToDisplayString())} {(x.Item.MatchedParam.methodParam is null ? $"__{x.Index + 1}" : x.Item.MatchedParam.methodParam.Name)}"
                                        )}}) { {{method.Name}}({{string.Join(", ",
                                                    from x in paramsWithCast
                                                    select (
                                                        x.castFromType is null ? "" : $"({x.methodParam.Type})"
                                                    ) + x.methodParam.Name
                                                )}});
                                            }
                                            """
                                    ).JoinNewLine()
                            ).JoinDoubleNewLine().IndentWOF(2)}}
                        }
                    }
                    """);
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
    static string? GetVisibilityPrefix(string DefaultPrefix, TypedConstant Value, GeneratorVisibility defaultVisibility)
    {
        try
        {
            GeneratorVisibility propertyVisibility = (GeneratorVisibility)(byte)Value.Value!;
            return GetVisiblityPrefix(DefaultPrefix, propertyVisibility);
        }
        catch
        {
            return GetVisiblityPrefix(DefaultPrefix, defaultVisibility);
        }
    }
}