using SolisCore.Lexing;
using System;
using System.Collections.Generic;
using System.Text;

namespace SolisCore.Parser
{
    public class ASTNode
    {

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
