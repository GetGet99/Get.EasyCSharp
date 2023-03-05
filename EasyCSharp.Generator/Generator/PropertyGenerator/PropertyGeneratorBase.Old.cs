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
[AddAttributeConverter(typeof(PropertyAttribute))]
partial class PropertyGeneratorBase<T> : GeneratorBase<FieldAttributeSyntaxReceiver> where T : PropertyAttribute
{   
    protected override FieldAttributeSyntaxReceiver ConstructSyntaxReceiver()
        => new(typeof(T).FullName);

    protected override void OnExecute(GeneratorExecutionContext context, FieldAttributeSyntaxReceiver SyntaxReceiver)
    {
        foreach
            (
                var FieldSymbols in
                SyntaxReceiver.Fields
                .GroupBy<IFieldSymbol, INamedTypeSymbol>
                (f => f.ContainingType, SymbolEqualityComparer.Default)
            )
        {
            var ClassSymbol = FieldSymbols.Key;
            context.AddSource(FileNameOverride($"{ClassSymbol.ContainingNamespace}.{ClassSymbol.Name}"), $$"""
                #nullable enable
                using System.Runtime.CompilerServices;
                namespace {{ClassSymbol.ContainingNamespace}}
                {
                    partial class {{ClassSymbol.Name}}{{ClassHeadLogic(context, ClassSymbol)}}
                    {
                        // Pregenerate Logic For Subclass Generator Override
                        {{PreGeneratorRun(ClassSymbol, context).IndentWOF(1)}}

                        {{
                            (
                                from field in FieldSymbols
                                select (
                                    from attribute in field.GetAttributes()
                                    let attr = AttributeDataToPropertyAttribute(attribute, context.Compilation)
                                    let PropertyName = ChooseName(field.Name, attr.PropertyName)
                                    let InlineText = attr.AgressiveInline ? "[MethodImpl(MethodImplOptions.AggressiveInlining)]" : "// Inline is false"
                                    select
                                    attr.Visibility is GeneratorVisibility.DoNotGenerate ?
                                    $"// {field.ToDisplayString()} has Property's Visibility set to {GeneratorVisibility.DoNotGenerate}" :
                                    $$"""
                                    // Generated For {{field.ToDisplayString()}}

                                    // BeforePropertyLogic for Overridden class
                                    {{BeforePropertyLogic(context, field, PropertyName, attribute)}}

                                    /// <summary>
                                    /// <inheritdoc cref="{{field.ToDisplayString()}}"/>
                                    /// </summary>
                                    {{
                                        GetVisiblity(attr.Visibility)
                                    }}{{
                                        (attr.OverrideKeyword ? "override " : "")
                                    }}{{(attr.CustomType ?? field.Type).FullName()}} {{PropertyName}} {
                                        
                                        {{( // Get
                                            attr.GetVisibility is GeneratorVisibility.DoNotGenerate ? "// Get is not generated according to the attribute" :
                                            $$"""
                                            {{InlineText}}
                                            {{GetVisiblity(attr.GetVisibility)}}get {
                                                // General Get logic
                                                return {{GetCustomLogicOverride(field, PropertyName, attr.CustomGetExpression ?? $"{field.Name}")}};
                                            }
                                            """.IndentWOF(1)
                                        )}}
                                        {{( // Set
                                            attr.SetVisibility is GeneratorVisibility.DoNotGenerate ? "// Set is not generated according to the attribute" :
                                            $$"""
                                            {{InlineText}}
                                            {{GetVisiblity(attr.SetVisibility)}}set {
                                                // User Defined OnBeforeChanged attribute
                                                {{(attr.OnBeforeChanged is null ? "// OnChanged is not set" : $"{attr.OnBeforeChanged}();")}}
                                                
                                                // General Set logic
                                                {{field.Name}} = {{attr.CustomSetExpression ?? "value"}};
                                                
                                                // OnSet Logic For Subclass Generator Override
                                                {{OnSet(field, PropertyName, attribute).IndentWOF(1)}}
                                                
                                                // User Defined OnChanged attribute
                                                {{(attr.OnChanged is null ? "// OnChanged is not set" : $"{attr.OnChanged}();")}}
                                            }
                                            """.IndentWOF(1)
                                        )}}
                                    }
                                    
                                    // End Generated For {{field.ToDisplayString()}}
                                    """
                                    ).JoinDoubleNewLine()
                            ).JoinDoubleNewLine().IndentWOF(2)

                        //
                        }}
                    }
                }
                """);
        }
    }
    protected virtual string GetCustomLogicOverride(IFieldSymbol FieldSymbol, string PropertyName, string Original) => Original;
    protected virtual string FileNameOverride(string ClassName) => $"{ClassName}.GeneratedProperty.g.cs";
    protected virtual string ClassHeadLogic(GeneratorExecutionContext context, INamedTypeSymbol classSymbol) => "";
    protected virtual string BeforePropertyLogic(GeneratorExecutionContext context, IFieldSymbol fieldSymbol, string PropertyName, AttributeData attributeData) => "// BeforePropertyLogic Not Overrideen";
    protected virtual string OnSet(IFieldSymbol FieldSymbol, string PropertyName, AttributeData attributeData) => "// OnSet Not Overriden";
    protected virtual string PreGeneratorRun(INamedTypeSymbol classSymbol, GeneratorExecutionContext context)
        => "// PreGeneratorRun Not Overridden";

    static string ChooseName(string fieldName, string? overridenNameOpt)
    {
        if (overridenNameOpt is not null) return overridenNameOpt;

        fieldName = fieldName.TrimStart('_');
        if (fieldName.Length == 0)
            return string.Empty;

        if (fieldName.Length == 1)
            return fieldName.ToUpper();

        return fieldName.Substring(0, 1).ToUpper() + fieldName.Substring(1);
    }
    static string? GetVisiblity(GeneratorVisibility propertyVisibility)
        => propertyVisibility switch
        {
            GeneratorVisibility.Default => "",
            GeneratorVisibility.DoNotGenerate => null,
            GeneratorVisibility.Public => $"public ",
            GeneratorVisibility.Private => $"private ",
            GeneratorVisibility.Protected => $"protected ",
            _ => throw new ArgumentOutOfRangeException()
        };
}