using SolisCore.Lexing;
using System;
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

    // Ordered roughly in precedence order
    public enum OperatorKind
    {
        Call,
        Member,
        Index,

        // same prec
        UnaryPlus,
        UnaryMinus,
        UnaryLogicalNegate,
        UnaryBitwiseNegate,

        // same prec
        BinaryMultiply,
        BinaryDivide,
        BinaryModulos,

        // same prec
        BinaryPlus,
        BinaryMinus,

        // or -> xor -> and, prec matters
        BinaryBitwiseAnd,
        BinaryBitwiseXor,
        BinaryBitwiseOr,

        // prec all the same
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
        Equal,
        NotEqual,

        // or -> and, prec matters
        LogicalAnd,
        LogicalOr,
    }

    public enum Precendence
    {
        LogicalOr,
        LogicalAnd,
        BinaryComparators,
        BinaryBitwiseOr,
        BinaryBitwiseXor,
        BinaryBitwiseAnd,
        BinaryMultiplicative,
        BinaryAdditive,
        Unary,
        // TODO: Prec on this is weird
        Member,
        CallAndIndex,

        // last member
        MaxPrecendence,
    }

    public static class OperatorKindExtensions
    {
        public static Precendence GetOperatorPrecedence(OperatorKind op)
        {
            return op switch
            {
                OperatorKind.Call => Precendence.CallAndIndex,
                OperatorKind.Index => Precendence.CallAndIndex,
                OperatorKind.Member => Precendence.Member,
                OperatorKind.UnaryPlus => Precendence.Unary,
                OperatorKind.UnaryMinus => Precendence.Unary,
                OperatorKind.UnaryLogicalNegate => Precendence.Unary,
                OperatorKind.UnaryBitwiseNegate => Precendence.Unary,
                OperatorKind.BinaryMultiply => Precendence.BinaryMultiplicative,
                OperatorKind.BinaryDivide => Precendence.BinaryMultiplicative,
                OperatorKind.BinaryModulos => Precendence.BinaryMultiplicative,
                OperatorKind.BinaryPlus => Precendence.BinaryAdditive,
                OperatorKind.BinaryMinus => Precendence.BinaryAdditive,
                OperatorKind.BinaryBitwiseAnd => Precendence.BinaryBitwiseAnd,
                OperatorKind.BinaryBitwiseXor => Precendence.BinaryBitwiseXor,
                OperatorKind.BinaryBitwiseOr => Precendence.BinaryBitwiseOr,
                OperatorKind.LessThan => Precendence.BinaryComparators,
                OperatorKind.GreaterThan => Precendence.BinaryComparators,
                OperatorKind.LessThanOrEqual => Precendence.BinaryComparators,
                OperatorKind.GreaterThanOrEqual => Precendence.BinaryComparators,
                OperatorKind.Equal => Precendence.BinaryComparators,
                OperatorKind.NotEqual => Precendence.BinaryComparators,
                OperatorKind.LogicalAnd => Precendence.LogicalAnd,
                OperatorKind.LogicalOr => Precendence.LogicalOr,
                _ => throw new NotImplementedException(op + " is not yet implemented"),
            };
        }
    }

    public class FunctionArg : ASTNode
    {
        public Token Name { get; }

        public FunctionArg(Token name)
        {
            Name = name;
        }
    }

    public class FunctionDeclaration : ASTNode
    {
        public List<FunctionArg> Args { get; }
        public Token? Identifier { get; }
        public StatementBody Body { get; }

        public FunctionDeclaration(List<FunctionArg> args, Token? identifier, StatementBody body)
        {
            Args = args;
            Identifier = identifier;
            Body = body;
        }
    }
}
