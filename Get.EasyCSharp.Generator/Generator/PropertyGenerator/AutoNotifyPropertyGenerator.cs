using System.Collections.Generic;
using System.ComponentModel;
using CopySourceGenerator;
using Get.EasyCSharp.GeneratorTools.SyntaxCreator.Expression;
using Get.EasyCSharp.GeneratorTools.SyntaxCreator.Lines;
using Get.EasyCSharp.GeneratorTools.SyntaxCreator.Members;
using Microsoft.CodeAnalysis;

namespace Get.EasyCSharp.Generator.PropertyGenerator;

partial class AutoNotifyPropertyGeneratorBase<T> : PropertyGeneratorBase<T> where T : AutoNotifyPropertyAttribute
{
    protected override void OnSet(LinkedList<ILine> Lines, IFieldSymbol symbol, string PropertyName, AttributeData data, Compilation compilation)
    {
        Lines.AddLast(
            new MethodCall("this.PropertyChanged?.Invoke",
                new(Expressions.This),
                new(new CustomExpression($"""
                new {new FullType($"global::{typeof(PropertyChangedEventArgs).FullName}").StringRepresentaion}(
                    nameof({PropertyName})
                )
                """))).EndLine()
        );
    }
}
[CopySource("AttributeSource", typeof(AutoNotifyPropertyAttribute))]
[Generator]
partial class AutoNotifyPropertyGenerator : AutoNotifyPropertyGeneratorBase<AutoNotifyPropertyAttribute>
{
    protected override void OnInitialize(IncrementalGeneratorPostInitializationContext context)
    {
        base.OnInitialize(context);
        context.AddSource($"{typeof(AutoNotifyPropertyAttribute).FullName}.g.cs", AttributeSource);
    }
}