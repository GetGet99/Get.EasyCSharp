using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CopySourceGenerator;
using EasyCSharp.GeneratorTools;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EasyCSharp;
[CopySource("AttributeSource", typeof(PropertyAttribute))]
public abstract partial class PropertyGeneratorBase : GeneratorBase<FieldAttributeSyntaxReceiver>
{
    protected override FieldAttributeSyntaxReceiver ConstructSyntaxReceiver() => new(AttributeTypeName);

    protected override void OnInitialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(x => x.AddSource($"{typeof(PropertyAttribute).FullName}.g.cs", AttributeSource));
    }


    protected virtual bool CustomOnChange => false;
    protected virtual string AttributeTypeName => typeof(PropertyAttribute).FullName;
    protected virtual string FileName(string ClaseName) => $"{ClaseName}.GeneratedProperty.g.cs";
    protected virtual string ClassHeadLogic(GeneratorExecutionContext context, INamedTypeSymbol classSymbol) => "";
    protected virtual string PreGeneratorRunContentLogic(INamedTypeSymbol classSymbol, GeneratorExecutionContext context, string Indent) => "";
    protected virtual string? BeforeHeadLogic(IFieldSymbol fieldSymbol, string propertyName, string Accessibility, string? OnChange) => null;
    protected virtual string HeadLogic(IFieldSymbol fieldSymbol, string propertyName) => $"{fieldSymbol.Type} {propertyName}";
    protected virtual string GetLogic(IFieldSymbol fieldSymbol, string PropertyName, string Indent) => $"return {fieldSymbol.Name};";
    protected virtual string SetLogic(IFieldSymbol fieldSymbol, string PropertyName, string Indent) => $"{fieldSymbol.Name} = value;";

    readonly static PropertyAttribute DefaultPropertyAttribute = new();

    protected override void OnExecute(GeneratorExecutionContext context, FieldAttributeSyntaxReceiver receiver)
    {
        INamedTypeSymbol? propertySymbol = context.Compilation.GetTypeByMetadataName(AttributeTypeName);

        if (propertySymbol is null)
            throw new NotImplementedException($"{typeof(PropertyAttribute).FullName} is not implemented");

        foreach
            (
                var group in
                receiver.Fields.GroupBy<IFieldSymbol, INamedTypeSymbol>
                (f => f.ContainingType, SymbolEqualityComparer.Default)
            )
        {
            string classSource = ProcessClass(group.Key, group.ToList(), propertySymbol, context);
            context.AddSource(FileName(group.Key.Name), SourceText.From(classSource, Encoding.UTF8));
        }
    }

    string ProcessClass(INamedTypeSymbol classSymbol, List<IFieldSymbol> fields, ISymbol propertySymbol, GeneratorExecutionContext context)
    {
        if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
            return $"// Field '{propertySymbol}' is on the namespace, which is not a valid place to generate property";

        string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

        // begin building the generated source
        StringBuilder source = new($@"
using System.Runtime.CompilerServices;
namespace {namespaceName}
{{
    public partial class {classSymbol.Name}{ClassHeadLogic(context, classSymbol)}
    {{
" + PreGeneratorRunContentLogic(classSymbol, context, "        "));

        bool HasAnnotation = false;
        // create properties for each field 
        foreach (IFieldSymbol fieldSymbol in fields)
        {
            if (fieldSymbol.NullableAnnotation == NullableAnnotation.Annotated)
                HasAnnotation = true;
            ProcessField(source, fieldSymbol, propertySymbol);
        }

        source.Append("\n    }\n}");
        return (HasAnnotation ? "#nullable enable" : "") + source.ToString();
    }
    void ProcessField(StringBuilder source, IFieldSymbol fieldSymbol, ISymbol propertySymbol)
    {
        // get the name and type of the field
        string fieldName = fieldSymbol.Name;
        ITypeSymbol fieldType = fieldSymbol.Type;

        AttributeData attributeData =
            fieldSymbol.GetAttributes()
            .Single(ad => ad.AttributeClass?.Equals(propertySymbol, SymbolEqualityComparer.Default) ?? false);
        TypedConstant overridenNameOpt =
            attributeData.NamedArguments.SingleOrDefault(
                kvp => kvp.Key == nameof(DefaultPropertyAttribute.PropertyName)
            ).Value;
        //Debugger.Launch();
        string propertyName = ChooseName(fieldName, overridenNameOpt);
        if (propertyName.Length == 0 || propertyName == fieldName)
        {
            //TODO: issue a diagnostic that we can't process this field
            return;
        }
        string? Suffix;
        bool Override;
        if (attributeData.NamedArguments.SingleOrDefault(
                kvp => kvp.Key == nameof(DefaultPropertyAttribute.OverrideKeyword)
            ).Value.Value is bool outboolean2)
            Override = outboolean2;
        else Override = DefaultPropertyAttribute.OverrideKeyword;

        string? PropertyHead =
            (Suffix = GetSuffix(
                (Override ? "override " : "") + HeadLogic(fieldSymbol, propertyName),
                attributeData.NamedArguments.SingleOrDefault(
                    kvp => kvp.Key == nameof(DefaultPropertyAttribute.Visibility)
                ).Value,
                DefaultPropertyAttribute.Visibility
            )) ?? ((Override ? "override " : "") + HeadLogic(fieldSymbol, propertyName));
        string? OnChange =
            attributeData.NamedArguments.SingleOrDefault(
                kvp => kvp.Key == nameof(DefaultPropertyAttribute.OnChanged)
            ).Value.Value?.ToString();
        bool Inline;
        if (attributeData.NamedArguments.SingleOrDefault(
                kvp => kvp.Key == nameof(DefaultPropertyAttribute.AgressiveInline)
            ).Value.Value is bool outboolean)
            Inline = outboolean;
        else Inline = DefaultPropertyAttribute.AgressiveInline;
        string? GetVisiblity =
            GetSuffix(
                "get",
                attributeData.NamedArguments.SingleOrDefault(
                    kvp => kvp.Key == nameof(DefaultPropertyAttribute.GetVisibility)
                ).Value,
                DefaultPropertyAttribute.GetVisibility
            );
        string? SetVisiblity =
            GetSuffix(
                "set",
                attributeData.NamedArguments.SingleOrDefault(
                    kvp => kvp.Key == nameof(DefaultPropertyAttribute.SetVisibility)
                ).Value,
                DefaultPropertyAttribute.SetVisibility
            );

        string? BeforePropertyCraetion =
            BeforeHeadLogic(fieldSymbol, propertyName, (Suffix?.Split(' ')[0] + " ") ?? "", OnChange);
        if (CustomOnChange) OnChange = null;
#if DEBUG
        //if (!Debugger.IsAttached)
        //{
        //    Debugger.Launch();
        //}
#endif
        if (BeforePropertyCraetion is not null)
            source.Append("\n" + BeforePropertyCraetion.Indent(2));
        source.Append($@"
        /// <summary>
        /// <inheritdoc cref=""{fieldName}""/>
        /// </summary>
        {PropertyHead}
        {{");
        if (GetVisiblity is not null)
            source.Append($@"{(
                Inline ? $@"
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
  " : "  "
)}          {GetVisiblity}
            {{
                {GetLogic(fieldSymbol, propertyName, "                ")}
            }}");
        if (SetVisiblity is not null)
            source.Append($@"{(
                Inline ? $@"
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
  " : "  "
)}          {SetVisiblity}
            {{
                {SetLogic(fieldSymbol, propertyName, "                ")}{(
                OnChange is null ? "\n  " : $@"
                {OnChange}();
  "
)}          }}");
        source.Append("\n        }");


    }
    static string ChooseName(string fieldName, TypedConstant overridenNameOpt)
    {
        if (!overridenNameOpt.IsNull)
        {
            return overridenNameOpt.Value?.ToString() ?? "";
        }

        fieldName = fieldName.TrimStart('_');
        if (fieldName.Length == 0)
            return string.Empty;

        if (fieldName.Length == 1)
            return fieldName.ToUpper();

        return fieldName.Substring(0, 1).ToUpper() + fieldName.Substring(1);
    }
    static string? GetSuffix(string Suffix, TypedConstant Value, PropertyVisibility defaultVisibility)
    {
        try
        {
            PropertyVisibility propertyVisibility = (PropertyVisibility)(byte)Value.Value!;
            return GetSuffix(Suffix, propertyVisibility);
        }
        catch
        {
            return GetSuffix(Suffix, defaultVisibility);
        }
    }
    static string? GetSuffix(string Suffix, PropertyVisibility propertyVisibility)
        => propertyVisibility switch
        {
            PropertyVisibility.Default => Suffix,
            PropertyVisibility.DoNotGenerate => null,
            PropertyVisibility.Public => $"public {Suffix}",
            PropertyVisibility.Private => $"private {Suffix}",
            PropertyVisibility.Protected => $"protected {Suffix}",
            _ => throw new ArgumentOutOfRangeException()
        };

    
}