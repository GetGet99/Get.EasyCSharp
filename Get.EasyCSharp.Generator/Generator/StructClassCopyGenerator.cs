//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Security;
//using System.Text;
//using EasyCSharp.GeneratorTools;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using CopySourceGenerator;
//namespace EasyCSharp.Generator.Generator
//{
//    [CopySource("TypeCopyAttributeAttributes", typeof(TypeCopyAttribute))]
//    [AddAttributeConverter(typeof(TypeCopyAttribute), ParametersAsString = "typeof(bool)")]
//    [Generator]
//    partial class StructClassCopyGenerator : AttributeBaseGenerator<
//        TypeCopyAttribute,
//        StructClassCopyGenerator.TypeCopyAttributeWarpper,
//        TypeDeclarationSyntax,
//        ITypeSymbol
//    >
//    {
//        static readonly string MethodImplAttr = typeof(System.Runtime.CompilerServices.MethodImplAttribute).FullName;
//        static readonly string AggressiveInliningValue = $"{typeof(System.Runtime.CompilerServices.MethodImplOptions).FullName}.{nameof(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)}";

//        protected override void OnInitialize(IncrementalGeneratorPostInitializationContext context)
//        {
//            context.AddSource($"EasyCSharp.TypeCopyGeneratorAttributes.g.cs", TypeCopyAttributeAttributes);
//        }

//        protected override TypeCopyAttributeWarpper TransformAttribute(AttributeData attributeData, Compilation compilation)
//            => AttributeDataToTypeCopyAttribute(attributeData, compilation);
        
//        protected override string? OnPointVisit(GeneratorSyntaxContext genContext, TypeDeclarationSyntax syntaxNode, ITypeSymbol symbol, (AttributeData Original, TypeCopyAttributeWarpper Wrapper)[] attributeData)
//        {
//            return
//                GetCode(symbol.ContainingNamespace, attributeData, genContext.SemanticModel.Compilation)
//                .JoinNewLine();
//        }
//        IEnumerable<string> GetCode(INamespaceSymbol cls, (AttributeData Original, TypeCopyAttributeWarpper Wrapper)[] attributeDatas, Compilation compilation)
//        {
//            foreach (var (_, attr) in attributeDatas) {
//                var type = attr.Type;
//                foreach (var members in type.GetMembers())
//                {

//                }
//            }
//        }
//        static string? GetVisiblityPrefix(string DefaultPrefix, GeneratorVisibility propertyVisibility)
//            => propertyVisibility switch
//            {
//                GeneratorVisibility.Default => DefaultPrefix,
//                GeneratorVisibility.DoNotGenerate => null,
//                GeneratorVisibility.Public => $"public",
//                GeneratorVisibility.Private => $"private",
//                GeneratorVisibility.Protected => $"protected",
//                _ => throw new ArgumentOutOfRangeException()
//            };
//    }
//}
