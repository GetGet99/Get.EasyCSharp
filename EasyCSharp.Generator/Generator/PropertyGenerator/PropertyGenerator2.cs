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

namespace EasyCSharp.Generator.Generator
{
    [Generator]
    class PropertyGenerator2 : PropertyGeneratorBase2<PropertyAttribute> { }
    [CopySource("PropertyAttributeText", typeof(PropertyAttribute))]
    [AddAttributeConverter(typeof(PropertyAttribute))]
    partial class PropertyGeneratorBase2<T> : AttributeBaseGenerator<
        T,
        PropertyGeneratorBase2<T>.PropertyAttributeWarpper,
        FieldDeclarationSyntax,
        IFieldSymbol
    >
        where T : PropertyAttribute
    {
        protected override void Initialize(IncrementalGeneratorPostInitializationContext context)
        {
            System.Diagnostics.Debugger.Break();
            //context.AddSource($"EasyCSharp.PropertyAttribute.g.cs", PropertyAttributeText);
        }

        protected override PropertyAttributeWarpper TransformAttribute(AttributeData attributeData, Compilation compilation)
        {
            System.Diagnostics.Debugger.Break();
            return AttributeDataToPropertyAttribute(attributeData, compilation);
        }

        protected override string? OnPointVisit(GeneratorSyntaxContext genContext, FieldDeclarationSyntax syntaxNode, IFieldSymbol symbol, PropertyAttributeWarpper[] attributeData)
        {
            return GetCode(symbol, attributeData, genContext.SemanticModel.Compilation).JoinNewLine();
        }
        IEnumerable<string> GetCode(IFieldSymbol field, PropertyAttributeWarpper[] attributeDatas, Compilation compilation)
        {
            foreach (var attr in attributeDatas)
            {
                if (attr.Visibility is GeneratorVisibility.DoNotGenerate) continue;

                yield return new Property(GetSyntaxVisiblity(attr.Visibility), new(field.ContainingType), ChooseName(field.Name, attr.PropertyName))
                {
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
                            new Assign(new(field.Name), new CustomExpression(attr.CustomSetExpression ?? "value")).EndLine()
                        }

                    }
                }.StringRepresentaion;
            }
            yield break;
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
    }
}
