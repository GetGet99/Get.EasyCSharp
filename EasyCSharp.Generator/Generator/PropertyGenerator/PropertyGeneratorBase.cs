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
using EasyCSharp.GeneratorTools.SyntaxCreator;
using EasyCSharp.GeneratorTools.SyntaxCreator.Members;
using EasyCSharp.GeneratorTools.SyntaxCreator.Attributes;
using EasyCSharp.GeneratorTools.SyntaxCreator.Lines;
using EasyCSharp.GeneratorTools.SyntaxCreator.Expression;
using System.Diagnostics;

namespace EasyCSharp;

[AddAttributeConverter(typeof(PropertyAttribute))]
partial class PropertyGeneratorBase<T> : AttributeBaseGenerator<
    T,
    PropertyGeneratorBase<T>.PropertyAttributeWarpper,
    FieldDeclarationSyntax,
    IFieldSymbol
>
    where T : PropertyAttribute
{
    protected override bool CountAttributeSubclass => false;

    protected override PropertyAttributeWarpper TransformAttribute(AttributeData attributeData, Compilation compilation)
    {
        return AttributeDataToPropertyAttribute(attributeData, compilation);
    }

    protected override string? OnPointVisit(GeneratorSyntaxContext genContext, FieldDeclarationSyntax syntaxNode, IFieldSymbol symbol, (AttributeData Original, PropertyAttributeWarpper Wrapper)[] attributeData)
    {
        return GetCode(symbol, attributeData, genContext.SemanticModel.Compilation).JoinNewLine();
    }
    IEnumerable<string> GetCode(IFieldSymbol field, (AttributeData Original, PropertyAttributeWarpper Wrapper)[] attributeDatas, Compilation compilation)
    {
        foreach (var(originalattr, attr) in attributeDatas)
        {
            if (attr.Visibility is GeneratorVisibility.DoNotGenerate) continue;
            var propertyName = ChooseName(field.Name, attr.PropertyName);
            yield return new Property(GetSyntaxVisiblity(attr.Visibility), new(field.Type), propertyName)
            {
                Documentation = new CustomDocumentation(
                    $"""
                    /// <summary>
                    /// <inheritdoc cref="{field.Name}"/>
                    /// </summary>
                    """
                ),
                Override = attr.OverrideKeyword,
                Get =
                {
                    Visibility = GetSyntaxVisiblity(attr.GetVisibility),
                    Attributes =
                    {
                        () => attr.AgressiveInline ?
                            new CustomAttribute("[MethodImpl(MethodImplOptions.AggressiveInlining)]") :
                            null
                    },
                    Code =
                    {
                        new Return(new Variable(attr.CustomGetExpression ?? field.Name))
                    }
                },
                Set =
                {
                    Visibility = GetSyntaxVisiblity(attr.SetVisibility),
                    Attributes =
                    {
                        () => attr.AgressiveInline ?
                            new CustomAttribute("[MethodImpl(MethodImplOptions.AggressiveInlining)]") :
                            null
                    },
                    Code =
                    {
                        x => {
                            if (attr.CheckForChanges)
                            {
                                var methodCall = attr.CustomComparisonCodeForChanges ??
                                (
                                    attr.ReferenceCompareForChanges ?? !field.Type.IsValueType ?
                                    $"object.ReferenceEquals({field.Name}, value)" : $"global::System.Collections.Generic.EqualityComparer<{field.Type.FullName()}>.Default.Equals({field.Name}, value)"
                                );
                                x.AddLast(new CustomLine(
                                    $"""
                                    if ({methodCall}) return;
                                    """
                                ));
                            }
                        },
                        new Assign(new(field.Name), new CustomExpression(attr.CustomSetExpression ?? "value")).EndLine(),
                        () => attr.OnChanged is not null ? new MethodCall(attr.OnChanged).EndLine() : null,
                        list => OnSet(list, field, propertyName, originalattr)
                    }
                }
            }.StringRepresentaion;
        }
        yield break;
    }
    protected virtual void OnSet(LinkedList<ILine> Lines, IFieldSymbol symbol, string PropertyName, AttributeData data) { }
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
    static SyntaxVisibility GetSyntaxVisiblity(GeneratorVisibility propertyVisibility)
        => propertyVisibility switch
        {
            GeneratorVisibility.Default => SyntaxVisibility.Default,
            GeneratorVisibility.DoNotGenerate => SyntaxVisibility.DoNotGenerate,
            GeneratorVisibility.Public => SyntaxVisibility.Public,
            GeneratorVisibility.Private => SyntaxVisibility.Private,
            GeneratorVisibility.Protected => SyntaxVisibility.Protected,
            _ => throw new ArgumentOutOfRangeException()
        };
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
    protected override IEnumerable<IFieldSymbol> GetSymbols(GeneratorSyntaxContext genContext, FieldDeclarationSyntax syntaxNode)
    {
        foreach (var variable in syntaxNode.Declaration.Variables)
            if (genContext.SemanticModel.GetDeclaredSymbol(variable) is IFieldSymbol symbol)
                yield return symbol;
    }
}
