using Microsoft.CodeAnalysis;
using CopySourceGenerator;

namespace Get.EasyCSharp.Generator.PropertyGenerator;

[Generator]
[CopySource("PropertyAttributeText", typeof(PropertyAttribute))]
partial class PropertyGenerator : PropertyGeneratorBase<PropertyAttribute> {
    protected override void OnInitialize(IncrementalGeneratorPostInitializationContext context)
    {
        base.OnInitialize(context);
        context.AddSource($"EasyCSharp.PropertyAttribute.g.cs", PropertyAttributeText);
    }
}