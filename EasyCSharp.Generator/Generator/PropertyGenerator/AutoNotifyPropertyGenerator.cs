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
public partial class AutoNotifyPropertyGenerator : PropertyGeneratorBase
{
    protected override void OnInitialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(x => x.AddSource($"{typeof(AutoNotifyPropertyAttribute).FullName}.g.cs", AttributeSource));
    }

    protected override string AttributeTypeName => typeof(AutoNotifyPropertyAttribute).FullName;
    protected override string FileName(string ClaseName) => $"{ClaseName}.GeneratedAutoNotifyProperty.g.cs";
    protected override string ClassHeadLogic(GeneratorExecutionContext context, INamedTypeSymbol classSymbol)
    {
        return $" : {context.Compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName)?.ToDisplayString() ?? "// INotifyPropertyChanged type is not found on the assembly"}";
    }
    protected override string GetLogic(IFieldSymbol fieldSymbol, string PropertyName, string Indent)
        => $"return {fieldSymbol.Name};";
    protected override string SetLogic(IFieldSymbol fieldSymbol, string PropertyName, string Indent)
        => $@"{fieldSymbol.Name} = value;
{Indent}this.PropertyChanged?.Invoke(
{Indent}    this,
{Indent}    new System.ComponentModel.PropertyChangedEventArgs(
{Indent}        nameof({PropertyName})
{Indent}    )
{Indent});";
    protected override string PreGeneratorRunContentLogic(INamedTypeSymbol classSymbol, GeneratorExecutionContext context, string Indent)
    {
        // if the class doesn't implement PropertyChanged already, implement it
        if (!classSymbol.GetMembers().Any(x => x.Name == "PropertyChanged"))
        {
            return $"{Indent}public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;";
        }
        return "";
    }
}