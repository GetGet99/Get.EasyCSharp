using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CopySourceGenerator;
using EasyCSharp.GeneratorTools.SyntaxCreator;
using EasyCSharp.GeneratorTools.SyntaxCreator.Expression;
using EasyCSharp.GeneratorTools.SyntaxCreator.Lines;
using EasyCSharp.GeneratorTools.SyntaxCreator.Members;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EasyCSharp;

[CopySource("AttributeSource", typeof(AutoNotifyPropertyAttribute))]
[Generator]
partial class AutoNotifyPropertyGenerator : PropertyGeneratorBase<AutoNotifyPropertyAttribute>
{
    protected override void OnInitialize(IncrementalGeneratorPostInitializationContext context)
    {
        base.OnInitialize(context);
        context.AddSource($"{typeof(AutoNotifyPropertyAttribute).FullName}.g.cs", AttributeSource);
    }

    protected override void OnSet(LinkedList<ILine> Lines, IFieldSymbol symbol, string PropertyName, AttributeData data)
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