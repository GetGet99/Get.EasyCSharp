namespace EasyCSharp.GeneratorTools.SyntaxCreator.Lines;

interface ILine : ISyntax { }

interface IComment : ILine { }
interface IDocumentation : IComment { }