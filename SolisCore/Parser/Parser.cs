using SolisCore.Lexing;
using System;
using System.Collections.Generic;

namespace SolisCore.Parser
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int idx;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public static StatementBody ParseTree(List<Token> tokens)
        {
            return new Parser(tokens).ParseStatementBody();
        }

        private StatementBody ParseStatementBody()
        {
            var result = new StatementBody();
            while (_tokens.Count > idx)
            {
                result.Statements.Add(ParseStatement());
            }

            return result;
        }

        private ASTNode? TryConsume(Func<bool> condition, Func<ASTNode> result)
        {
            if (condition()) return null;

            // move one token (note: this requires condition to be 1 token)
            idx++;
            return result();
        }

        private Token GetIdent()
        {
            if (_tokens[idx].Kind == TokenKind.Ident)
            {
                return _tokens[idx++];
            }
            else
            {
                throw new Exception("Requires identifier");
            }
        }

        private ASTNode ParseExpression()
        {
            return null;
        }

        private List<T> ParseList<T>(Func<bool> terminatingCondition, Func<bool> nextToken, Func<T> parseItem) where T : ASTNode
        {
            var result = new List<T>();
            while (!terminatingCondition())
            {
                result.Add(parseItem());
                if (!nextToken())
                {
                    // require terminating condition
                    if (!terminatingCondition())
                    {
                        throw new Exception("Expected terminating token");
                    }
                    break;
                }
                else
                {
                    idx++;
                    // we allow the next token to also be immediately terminated
                    // i.e. a(b, c,)
                    // we may remove this for some things in future...
                }
            }

            // terminating token
            idx++;
            
            return result;
        }

        private FunctionArg ParseArg()
        {
            return new FunctionArg(GetIdent());
        }

        private ASTNode ParseStatement()
        {
            // declarations
            if (_tokens[idx].Kind == TokenKind.Var || _tokens[idx].Kind == TokenKind.Const)
            {
                return new VariableDeclaration(
                    isConst: _tokens[idx++].Kind == TokenKind.Const,
                    GetIdent(),
                    TryConsume(() => _tokens[idx] is Token { Kind: TokenKind.AssignmentSymbol, Value: "=" }, ParseExpression));
            }
            
            // functions
            if (_tokens[idx].Kind == TokenKind.Fn)
            {
                idx++;

                Token? ident = null;
                if (_tokens[idx].Kind == TokenKind.Ident)
                {
                    ident = _tokens[idx++];
                }

                if (_tokens[idx++] is not { Kind: TokenKind.PunctuationSymbol, Value: "(" })
                {
                    throw new Exception("Functions need atleast an empty list of args");
                }
                return new FunctionDeclaration(
                    ParseList(
                        () => _tokens[idx] is { Kind: TokenKind.PunctuationSymbol, Value: ")" },
                        () => _tokens[idx] is { Kind: TokenKind.PunctuationSymbol, Value: "," },
                        ParseArg),
                    ident,
                    // TODO: needs { }
                    ParseStatementBody()
                    );
            }

            throw new Exception("unknown parsing");
        }
    }
}
