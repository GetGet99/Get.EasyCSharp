using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EasyCSharp.GeneratorTools;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EasyCSharp;
[Generator]
public class GeneratorToolsGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context) { }

    public void Initialize(GeneratorInitializationContext context)
    {
        
    }
}