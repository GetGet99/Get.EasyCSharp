using EasyCSharp.GeneratorTools.SyntaxCreator.Attributes;
using EasyCSharp.GeneratorTools.SyntaxCreator.Lines;
using System.Collections.Generic;
using System.Linq;

namespace EasyCSharp.GeneratorTools.SyntaxCreator.Members;

abstract class MethodBase : IMember
{
    protected MethodBase(SyntaxVisibility Visibility, FullType ReturnType, string Name, IEnumerable<ParameterDefinition>? Parameters = null)
    {
        this.Name = Name;
        this.Visibility = Visibility;
        this.ReturnType = ReturnType;
        if (Parameters is not null)
            foreach (var param in Parameters)
            {
                this.Parameters.AddLast(param);
            }
    }
    public string Name { get; }
    public FullType ReturnType { get; }
    public SyntaxVisibility Visibility { get; }
    public IDocumentation? Documentation { get; }
    public LinkedList<ILine> Code { get; } = new();
    public LinkedList<ParameterDefinition> Parameters { get; } = new();

    public string StringRepresentaion => ToString();

    protected virtual IEnumerable<string> GetKeywords()
    {
        if (Visibility.GetString() is string s)
            yield return s;
    }
    public override string ToString()
    {
        return $$"""
                {{Documentation?.StringRepresentaion ?? "// No Documentation was provided"}}
                {{GetKeywords().JoinWith(" ")}} {{ReturnType}} {{Name}}({{Parameters.Select(x => $"{x.Type} {x.Name}").JoinWith(", ")}}) {
                    {{Code.Select(x => x.StringRepresentaion).JoinNewLine().IndentWOF(1)}}
                }
                """;
    }
}