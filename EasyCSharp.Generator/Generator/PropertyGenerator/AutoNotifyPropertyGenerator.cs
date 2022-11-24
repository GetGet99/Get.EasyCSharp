using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CopySourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EasyCSharp;

[CopySource("AttributeSource", typeof(AutoNotifyPropertyAttribute))]
[Generator]
partial class AutoNotifyPropertyGenerator : PropertyGeneratorBase<AutoNotifyPropertyAttribute>
{
    protected override void OnInitialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(x => x.AddSource($"{typeof(AutoNotifyPropertyAttribute).FullName}.g.cs", AttributeSource));
    }

    protected override string FileNameOverride(string ClassName)
        => $"{ClassName}.GeneratedAutoNotifyProperty.g.cs";
    protected override string ClassHeadLogic(GeneratorExecutionContext context, INamedTypeSymbol classSymbol)
    {
        return $" : {context.Compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName)?.ToDisplayString() ?? "// INotifyPropertyChanged type is not found on the assembly"}";
    }
    protected override string OnSet(IFieldSymbol FieldSymbol, string PropertyName, AttributeData attributeData)
    => $"""
            this.PropertyChanged?.Invoke(
                this,
                new {typeof(PropertyChangedEventArgs).FullName}(
                    nameof({PropertyName})
                )
            );
            """;
    protected override string PreGeneratorRun(INamedTypeSymbol classSymbol, GeneratorExecutionContext context)
    {
        // if the class doesn't implement PropertyChanged already, implement it
        if (!classSymbol.GetMembers().Any(x => x.Name == "PropertyChanged"))
        {
            return $"public event {typeof(PropertyChangedEventHandler).FullName}? PropertyChanged;";
        }
        return "";
    }
}