
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using EasyCSharp.CustomGenerator;
using EasyCSharp.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using static EasyCSharp.CustomGeneratorGenerator;

namespace EasyCSharp;
[Generator]
public class CustomGeneratorGenerator : GeneratorBase<CustomGeneratorSyntaxReceiver>
{
    protected override CustomGeneratorSyntaxReceiver ConstructSyntaxReceiver()
        => new();
    public class CustomGeneratorSyntaxReceiver : ISyntaxContextReceiver
    {
        public CustomGeneratorSyntaxReceiver()
        {

        }
        //public List<IFieldSymbol> Fields { get; } = new List<IFieldSymbol>();
        public Dictionary<INamedTypeSymbol, List<IFieldSymbol>> Symbols = new(SymbolEqualityComparer.Default);
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            var ICustomGeneratorType = context.SemanticModel.Compilation
                            .GetTypeByMetadataName(typeof(CustomGeneratorBase).FullName);
            var BaseCustomGeneratorAttributeType = context.SemanticModel.Compilation
                            .GetTypeByMetadataName(typeof(BaseCustomGeneratorAttribute).FullName)
            if (ICustomGeneratorType is null) return;
            if (context.Node is ClassDeclarationSyntax classDeclarationSyntax)
            {
                // Get the symbol being declared by the field, and keep it if its annotated;
                if (
                        context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) is INamedTypeSymbol namedTypeSymbol &&
                        IsParentOf(
                            namedTypeSymbol,
                            ICustomGeneratorType
                        )
                    )
                {
                    Symbols.ContainsKey(namedTypeSymbol)
                    //var expr = (
                    //    (namedTypeSymbol.GetMembers()[0] as IMethodSymbol)?
                    //    .DeclaringSyntaxReferences[0].GetSyntax() as MethodDeclarationSyntax
                    //)?
                    //    .ExpressionBody?.Expression as InterpolatedStringExpressionSyntax;
                    //if (expr is null) return;
                    //var contents = expr.Contents.Select(
                    //    x =>
                    //    {
                    //        if (x is InterpolatedStringTextSyntax txt) return txt.ToFullString();
                    //        var expr = (x as InterpolationSyntax)?.Expression;
                    //        var model = context.SemanticModel;
                    //        Debugger.Launch();
                    //        return "";
                    //    }
                    //).ToArray();
                    Debugger.Launch();
                    //CompileMethod()
                }
            }
        }
        bool IsParentOf(INamedTypeSymbol? tester, INamedTypeSymbol type)
        {
            if (tester is null) return false;
            if (tester.Equals(type, SymbolEqualityComparer.Default)) return true;
            return IsParentOf(tester.BaseType, type);
        }

        static MetadataReference[] references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(CustomGeneratorGenerator).Assembly.Location)
        };
        // Code copied an dmodified from https://github.com/hermanussen/CompileTimeMethodExecutionGenerator
        private static Func<object[], string?> CompileMethod(MethodDeclarationSyntax method)
        {
            CSharpParseOptions options = method.SyntaxTree.Options as CSharpParseOptions ?? throw new ArgumentException("method.SyntaxTree.Options is not CSharpParseOptions");

            CSharpCompilation compilation = CSharpCompilation.Create(
                Path.GetRandomFileName(),
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(
                    SourceText.From(
                        $@"
using EasyCSharp;
using EasyCSharp.CustomGenerator;
public class C {{
    public {method.ReturnType} M({method.ParameterList.ToFullString()})
    {method.Body?.ToFullString() ?? method.ExpressionBody?.ToFullString()}
}}", Encoding.UTF8),
                    options) },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using var ms = new MemoryStream();
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                throw new Exception(string.Join("\r\n", failures.Select(f => $"{f.Id} {f.GetMessage()}")));
            }
            else
            {
                ms.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(ms.ToArray());

                Type type = assembly.GetType("C");
                object obj = Activator.CreateInstance(type);
                return args =>
                    type.InvokeMember("M",
                    BindingFlags.Default | BindingFlags.InvokeMethod,
                    null,
                    obj,
                    args)?.ToString();
            }
        }
    }
}