using SolisCore.Lexing;
using System;
using System.Collections.Generic;

namespace SolisCore.Parser
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private Token? Tok => idx < _tokens.Count ? _tokens[idx] : null;
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

        private StatementBody ParseStatementBodyBraced()
        {
            Consume(Token.Punctuation("{"));

            var result = new StatementBody();
            while (Tok != Token.Punctuation("}"))
            {
                if (Tok == null) throw new Exception($"Unexpected EOF, expected {Token.Punctuation("}")}");

                result.Statements.Add(ParseStatement());
            }
            Consume(Token.Punctuation("}"));

            return result;
        }

        private void Consume(Token condition)
        {
            if (Tok != condition) throw new Exception($"Expected {condition} got {Tok?.ToString() ?? "EOF"}");
            idx++;
        }

        private ASTNode? TryConsume(Token condition, Func<ASTNode> result)
        {
            if (Tok != condition) return null;

            // move one token (note: this requires condition to be 1 token)
            idx++;
            return result();
        }

        private Token? TryIdent()
        {
            if (Tok is { Kind: TokenKind.Identifier })
            {
                return _tokens[idx++];
            }
            else
            {
                return null;
            }
        }

        private Token ConsumeIdent()
        {
            return TryIdent() ?? throw new Exception("Requires identifier");
        }

        private Expression ParseExpressionAtom()
        {
            if (Tok is Token { Kind: TokenKind.Identifier, Value: string val })
            {
                return new AtomExpression(AtomKind.Identifier, val);
            }
            else if (Tok is Token { Kind: TokenKind kind, RealValue: var realVal } && Enum.TryParse<AtomKind>(kind.ToString(), out var atomKind))
            {
                return new AtomExpression(atomKind, realVal);
            }
            else if (Tok == Token.Punctuation("("))
            {
                // we have an (...) expression
                var expr = ParseExpression();
                expr.IsParenthesed = true;
                return expr;
            }

            throw new NotImplementedException("Unexpected atom expr");
        }

        private (Token op, Precendence prec, bool rightAssociative)? TryParseOperator()
        {
            if (Tok is Token { Kind: TokenKind.MathSymbol or TokenKind.ComparatorSymbol or TokenKind.BitwiseSymbol, Value: string opStr })
            {
                var op = opStr switch
                {
                    "+" => OperatorKind.BinaryPlus,
                    "-" => OperatorKind.BinaryMinus,
                    
                    "*" => OperatorKind.BinaryMultiply,
                    "/" => OperatorKind.BinaryDivide,
                    "%" => OperatorKind.BinaryModulos,
                    
                    "!" => OperatorKind.UnaryLogicalNegate,


                    "<" => OperatorKind.LessThan,
                    ">" => OperatorKind.GreaterThan,
                    "==" => OperatorKind.Equal,
                    "<=" => OperatorKind.LessThanOrEqual,
                    ">=" => OperatorKind.GreaterThanOrEqual,
                    "!=" => OperatorKind.NotEqual,

                    _ => throw new Exception(opStr + " is not a valid math symbol, invalid parsing logic")
                };
            }

            return null;
        }

        private Expression ParseExpressionAtPrecedence(int precedence)
        {
            // TODO:
            var atom = ParseExpressionAtom();
            return atom;
        }

        private Expression ParseExpression()
        {

            throw new Exception("Unexpected");
        }

        private List<T> ParseList<T>(Token end, Token next, Func<T> parse) where T : ASTNode
        {
            var result = new List<T>();
            while (true)
            {
                if (Tok == null) throw new Exception("Expected terminating token " + end + " got EOF");
                if (Tok == end) break;

                result.Add(parse());
                if (Tok != next)
                {
                    // require terminating condition
                    if (Tok != end)
                    {
                        throw new Exception($"Expected terminating token {end} got {Tok?.ToString() ?? "EOF"}");
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
            return new FunctionArg(ConsumeIdent());
        }

        private ASTNode ParseStatement()
        {
            // declarations
            if (Tok is { Kind: TokenKind.Const or TokenKind.Var })
            {
                return new VariableDeclaration(
                    isConst: _tokens[idx++].Kind == TokenKind.Const,
                    ConsumeIdent(),
                    TryConsume(Token.Assignment("="), ParseExpression));
            }
            // functions
            if (Tok is { Kind: TokenKind.Fn })
            {
                idx++;

                // function name is optional
                Token? ident = TryIdent();
                Consume(Token.Punctuation("("));

                return new FunctionDeclaration(
                    ParseList(end: Token.Punctuation(")"), next: Token.Punctuation(","), parse: ParseArg),
                    ident,
                    ParseStatementBodyBraced()
                );
            }

            return ParseExpression();

            throw new Exception("unknown parsing");
        }
    }
}
