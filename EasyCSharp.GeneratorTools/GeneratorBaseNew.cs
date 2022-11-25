#pragma warning disable IDE0240
#nullable enable
#pragma warning restore IDE0240
using Microsoft.CodeAnalysis;
using System.Threading;

namespace EasyCSharp.GeneratorTools;

public abstract class GeneratorBaseNew<T> : IIncrementalGenerator
{
    protected abstract bool FilterSyntaxNode(SyntaxNode Node, CancellationToken cancellationToken);
    protected abstract T TransformSyntaxNode(GeneratorSyntaxContext generatorSyntaxContext, CancellationToken cancellationToken);
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        
        var output = context.SyntaxProvider.CreateSyntaxProvider(FilterSyntaxNode, TransformSyntaxNode);
        var value = context.CompilationProvider.Combine(output.Collect());
        context.RegisterSourceOutput(output, (a, b) =>
        {
            
        });
    }
    protected abstract void Execute()
}
public abstract class GeneratorBaseNew : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        OnInitialize(context);
    }

    public void Execute(GeneratorExecutionContext context)
    {
        OnExecute(context);
    }
    protected virtual void OnInitialize(GeneratorInitializationContext context) { }
    protected virtual void OnExecute(GeneratorExecutionContext context) { }
}