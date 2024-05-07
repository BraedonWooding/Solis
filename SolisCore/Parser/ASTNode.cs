using SolisCore.Executors;
using SolisCore.Lexing;
using SolisCore.Typechecking;
using System.Collections;
using System.Collections.Generic;

namespace SolisCore.Parser
{
    public class ASTNode
    {
        public string AstKind => GetType().Name;
        public TypeAst? TypeAnnotation { get; set; }

        public ASTNode(TypeAst? typeAnnotation = null)
        {
            TypeAnnotation = typeAnnotation;
        }
    }

    public class StatementBody : ASTNode
    {
        public List<ASTNode> Statements { get; } = new();
    }

    public class VariableDeclaration : ASTNode
    {
        public bool IsConst { get; }
        public Token Identifier { get; }

        public string IdentifierValue => Identifier.SourceValue;
        public ASTNode? Expression { get; }

        public VariableDeclaration(bool isConst, Token identifier, TypeAst? type, ASTNode? expression = null) : base(type)
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
        If,
        While,
    }

    public interface IOperatorExpression : IEnumerable<Expression>
    {
        public OperatorKind Kind { get; }
    }

    public class BinaryOperatorExpression : Expression, IOperatorExpression
    {
        public BinaryOperatorExpression(OperatorKind kind, Expression target, Expression arg)
        {
            Kind = kind;
            Target = target;
            Arg = arg;
        }

        public OperatorKind Kind { get; }
        public Expression Target { get; }
        public Expression Arg { get; }

        public IEnumerator<Expression> GetEnumerator()
        {
            yield return Target;
            yield return Arg;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class UnaryOperatorExpression : Expression, IOperatorExpression
    {
        public UnaryOperatorExpression(OperatorKind kind, Expression target)
        {
            Kind = kind;
            Target = target;
        }

        public OperatorKind Kind { get; }
        public Expression Target { get; }

        public IEnumerator<Expression> GetEnumerator()
        {
            yield return Target;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class MemberOperatorExpression : Expression, IOperatorExpression
    {
        public MemberOperatorExpression(OperatorKind kind, Expression target, List<AtomExpression> path)
        {
            Kind = kind;
            Target = target;
            Path = path;
        }

        public OperatorKind Kind { get; }
        public Expression Target { get; }
        public List<AtomExpression> Path { get; }

        // note: we don't include Target in iteration
        public IEnumerator<Expression> GetEnumerator()
        {
            return Path.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CallOperatorExpression : Expression, IOperatorExpression
    {
        public CallOperatorExpression(OperatorKind kind, Expression target, List<Expression> args)
        {
            Kind = kind;
            Target = target;
            Args = args;
        }

        public OperatorKind Kind { get; }
        public Expression Target { get; }
        public List<Expression> Args { get; }

        // note: we don't include Target in iteration
        public IEnumerator<Expression> GetEnumerator()
        {
            return Args.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Expression : ASTNode
    {
        public bool IsParenthesed { get; set; } = false;
    }

    public class FunctionArg : ASTNode
    {
        public Token Name { get; }

        public FunctionArg(Token name, TypeAst? type) : base(type)
        {
            Name = name;
        }
    }

    public class ReturnExpression : ASTNode
    {
        public Expression Value;

        public ReturnExpression(Expression value)
        {
            Value = value;
        }
    }

    public class IfExpression : AtomExpression
    {
        public IfExpression(Expression condition, StatementBody body) : base(AtomKind.If, null)
        {
            Condition = condition;
            Body = body;
        }

        public Expression Condition { get; }
        public StatementBody Body { get; }
        public List<IfExpression> ElseIf { get; } = new();
        public StatementBody? Else { get; set; }
    }

    public class WhileExpression : AtomExpression
    {
        public WhileExpression(Expression condition, StatementBody body) : base(AtomKind.While, null)
        {
            Condition = condition;
            Body = body;
        }

        public Expression Condition { get; }
        public StatementBody Body { get; }
    }

    public class FunctionDeclaration : AtomExpression
    {
        public List<FunctionArg> Args { get; }
        public Token? Identifier { get; }
        public StatementBody Body { get; }
        public TypeAst? ReturnType { get; }

        public FunctionDeclaration(List<FunctionArg> args, Token? identifier, StatementBody body, TypeAst? returnType) : base(AtomKind.Function, null)
        {
            Args = args;
            Identifier = identifier;
            Body = body;
            ReturnType = returnType;
        }
    }
}
