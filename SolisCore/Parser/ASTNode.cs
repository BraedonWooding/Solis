using SolisCore.Lexing;
using System.Collections.Generic;
using System.Text;

namespace SolisCore.Parser
{
    public class ASTNode
    {
        public string Type => GetType().Name;
    }

    public class StatementBody : ASTNode
    {
        public List<ASTNode> Statements { get; } = new();
    }

    public class VariableDeclaration : ASTNode
    {
        public bool IsConst { get; }
        public Token Identifier { get; }
        public string IdentifierValue => Identifier.Value;
        public ASTNode? Expression { get; }

        public VariableDeclaration(bool isConst, Token identifier, ASTNode? expression = null)
        {
            IsConst = isConst;
            Identifier = identifier;
            Expression = expression;
        }
    }

    public class AtomExpression : Expression
    {
        public AtomExpression(AtomKind kind, object? value)
        {
            Kind = kind;
            Value = value;
        }

        public AtomKind Kind { get; }
        public object? Value { get; }
    }

    public enum AtomKind
    {
        Identifier,
        ValueInt,
        ValueFloat,
        ValueBool,
        ValueString,
        ValueChar,
        ValueNull,
        Function,
    }

    public class OperatorExpression : Expression
    {
        public OperatorExpression(OperatorKind kind, List<Expression> arguments)
        {
            Kind = kind;
            Arguments = arguments;
        }

        public OperatorKind Kind { get; }
        public List<Expression> Arguments { get; }
    }

    public class Expression : ASTNode
    {
        public bool IsParenthesed { get; set; } = false;
    }

    public class FunctionArg : ASTNode
    {
        public Token Name { get; }

        public FunctionArg(Token name)
        {
            Name = name;
        }
    }

    public class FunctionDeclaration : AtomExpression
    {
        public List<FunctionArg> Args { get; }
        public Token? Identifier { get; }
        public StatementBody Body { get; }

        public FunctionDeclaration(List<FunctionArg> args, Token? identifier, StatementBody body) : base(AtomKind.Function, null)
        {
            Args = args;
            Identifier = identifier;
            Body = body;
        }
    }
}
