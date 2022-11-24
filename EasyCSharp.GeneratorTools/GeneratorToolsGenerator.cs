using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CopySourceGenerator;
using EasyCSharp.GeneratorTools;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyCSharp.GeneratorTools;
[CopySource("sExtension", typeof(Extension))]
[CopySource("sGeneratorBase", typeof(GeneratorBase))]
[CopySource("sSyntaxReceiver", typeof(ClassAttributeSyntaxReceiver))]
[Generator]
public partial class GeneratorToolsGenerator : GeneratorBase
{
    protected override void OnInitialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(x => {
            x.AddSource($"{typeof(Extension).FullName}.g.cs", sExtension);
            x.AddSource($"{typeof(GeneratorBase).FullName}.g.cs", sGeneratorBase);
            x.AddSource($"EasyCSharp.GeneratorTools.SyntaxReceiver.g.cs", sSyntaxReceiver);
        });
    }
}
