using EasyCSharp.GeneratorTools.SyntaxCreator.Lines;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyCSharp.GeneratorTools.SyntaxCreator.Expression;

interface IExpression : ISyntax
{
}

interface ILineExpression : IExpression
{
    ILine AsLine();
}

record struct Variable(string VariableName) : IExpression
{
    public string StringRepresentaion => VariableName;

    //public static implicit operator Variable(string VariableName) => new(VariableName);
    public override string ToString() => StringRepresentaion;

    public void Invoke(params IExpression[] Expressions) => Invoke(Expressions.AsEnumerable());
    public void Invoke(IEnumerable<IExpression> Expressions)
    {
        
    }
}

record struct Assign(Variable Variable, IExpression Expression) : ILineExpression
{
    public string StringRepresentaion => $"{Variable} = {Expression}";
    public override string ToString() => StringRepresentaion;
    public ILine AsLine()
    {
        throw new System.NotImplementedException();
    }
}
