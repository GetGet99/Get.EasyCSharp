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

[Generator]
[CopySource("PropertyAttributeText", typeof(PropertyAttribute))]
partial class PropertyGenerator : PropertyGeneratorBase<PropertyAttribute> {
    protected override void OnInitialize(IncrementalGeneratorPostInitializationContext context)
    {
        base.OnInitialize(context);
        context.AddSource($"EasyCSharp.PropertyAttribute.g.cs", PropertyAttributeText);
    }
}