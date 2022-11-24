using Microsoft.CodeAnalysis;
using CopySourceGenerator;
namespace EasyCSharp;
[CopySource("PropertyAttributeSource", typeof(PropertyAttribute))]
[Generator]
partial class PropertyGenerator : PropertyGeneratorBase<PropertyAttribute>
{
    protected override void OnInitialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(x => x.AddSource($"{typeof(PropertyAttribute).FullName}.g.cs", PropertyAttributeSource));
    }
}