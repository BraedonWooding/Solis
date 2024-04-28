using SolisCore.Lexing;
using System;
using System.Collections.Generic;

namespace SolisCore.Parser
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private Token? Tok =>PeekToken(0);
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
                idx++;
                return new AtomExpression(AtomKind.Identifier, val);
            }
            else if (Tok is Token { Kind: TokenKind kind, RealValue: var realVal } && Enum.TryParse<AtomKind>(kind.ToString(), out var atomKind))
            {
                idx++;
                return new AtomExpression(atomKind, realVal);
            }
            else if (Tok == Token.Punctuation("("))
            {
                idx++;
                // we have an (...) expression
                var expr = ParseExpression();
                expr.IsParenthesed = true;
                Consume(Token.Punctuation(")"));
                return expr;
            }
            else if (Tok is { Kind: TokenKind.Fn })
            {
                idx++;

                if (TryIdent() is Token ident)
                {
                    // identifiers aren't allowed for functions as expressions
                    // this *may* change in the future, but it is just to clearly separate a "lambda" and a method.
                    throw new Exception("You can't supply an identifier for a function that is an expression (i.e. in a declaration0");
                }

                Consume(Token.Punctuation("("));

                return new FunctionDeclaration(
                    ParseList(end: Token.Punctuation(")"), next: Token.Punctuation(","), parse: ParseArg),
                    null,
                    ParseStatementBodyBraced()
                );
            }

            throw new NotImplementedException("Unexpected atom expr");
        }

        private (Token token, OperatorKind kind, Precendence prec, bool rightAssociative)? TryParseOperator()
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

                return (Tok.Value, op, op.GetOperatorPrecedence(), rightAssociative: false);
            }

            return null;
        }

        private Expression ParseExpressionAtPrecedence(Precendence currentPrecedence)
        {
            var expr = ParseExpressionAtom();
            var op = TryParseOperator();
            while (op is var (_, kind, precedence, rightAssociative) && (int)precedence >= (int)currentPrecedence)
            {
                // we need to move forward to "accept" the op
                idx++;

                var arg = ParseExpressionAtPrecedence(rightAssociative ? currentPrecedence : (Precendence)((int)currentPrecedence + 1));
                expr = new OperatorExpression(kind, new() { expr, arg });
                op = TryParseOperator();
            }

            return expr;
        }

        private Expression ParseExpression()
        {
            // handle some edge cases i.e. functions as expressions, if statement expressions (and other control flows)

            // we always start at 0 and climb up
            return ParseExpressionAtPrecedence(currentPrecedence: 0);
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

            // tricky part is distinguishing between an expression and a statement for a function
            // if the function has an identifier it has to be a statement.
            // For example: `fn A(b, c) { }()` is invalid syntax (only anonymous functions can be expressions)
            // so to determine that we peek to ensure next token is identifier
            if (Tok is { Kind: TokenKind.Fn } && PeekToken(1) is { Kind: TokenKind.Identifier } ident)
            {
                idx += 2;
                Consume(Token.Punctuation("("));

                return new FunctionDeclaration(
                    ParseList(end: Token.Punctuation(")"), next: Token.Punctuation(","), parse: ParseArg),
                    ident,
                    ParseStatementBodyBraced()
                );
            }

            var expr = ParseExpression();
            if (expr is FunctionDeclaration decl)
            {
                // if someone does the expression fn() {} (i.e. a blanket function definition)
                // then we will output a warning, maybe in future this can be a generic "unused result" or something
                // TODO: Make this a warning
                throw new Exception("Function declarations should have a name, this is an unused expression value");
            }

            return expr;
        }

        private Token? PeekToken(int lookahead)
        {
            // peek the next token (with optional lookahead) skipping all comments
            while (idx + lookahead < _tokens.Count)
            {
                if (_tokens[idx + lookahead] is { Kind: TokenKind.Comment })
                {
                    // tricky situation how do we want to deal with something like
                    // fn /* comment /* foo() {}
                    //
                    // in this case what I'm doing is only moving idx forwards *if*
                    // the lookahead is 0 which means the current token is a comment
                    // this is something we'll *always* want to skip.
                    // otherwise I will move the lookahead forwards to simulate it
                    //
                    // since otherwise PeekToken(1) in the above case would result in incorrect parsing
                    // if it was fn /* comment */ () {}, since the lookahead doesn't find an ident
                    // but it still increments the idx resulting in it now being /* comment */ () {}
                    // which will then become just () {}
                    if (lookahead == 0) idx++;
                    else lookahead++;
                }
                else
                {
                    return _tokens[idx + lookahead];
                }
            }
            return null;
        }
    }
}
