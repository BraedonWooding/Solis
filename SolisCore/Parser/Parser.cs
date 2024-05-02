using SolisCore.Lexing;
using System;
using System.Collections.Generic;

namespace SolisCore.Parser
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private Token? Tok => PeekToken(0);
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

        private T? TryConsume<T>(Token condition, Func<T> result)
        {
            if (Tok != condition) return default;

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

        private AtomExpression ParseAtomExpression()
        {
            if (Tok is Token { Kind: TokenKind.Identifier, SourceValue: string val })
            {
                idx++;
                return new AtomExpression(AtomKind.Identifier, val);
            }
            else if (Tok is Token { Kind: TokenKind kind, ParsedValue: var realVal } && Enum.TryParse<AtomKind>(kind.ToString(), out var atomKind))
            {
                idx++;
                return new AtomExpression(atomKind, realVal);
            }

            throw new NotImplementedException("Unexpected atom expr " + (Tok?.ToString() ?? "EOF"));
        }

        private Expression ParseSpecialCasesAndAtom()
        {
            if (Tok == Token.Punctuation("("))
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
            else if (Tok is { Kind: TokenKind.If })
            {
                idx++;

                var expr = new IfExpression(ParseExpression(), ParseStatementBodyBraced());
                // now for else if
                while (Tok == new Token(TokenKind.Else, default) && PeekToken(1) == new Token(TokenKind.If, default))
                {
                    idx++;
                    expr.ElseIf.Add(new IfExpression(ParseExpression(), ParseStatementBodyBraced()));
                }
                if (Tok == new Token(TokenKind.Else, default))
                {
                    // else statement
                    idx++;
                    expr.Else = ParseStatementBodyBraced();
                }
                // TODO: nicer errors around else {} else {} or else {} else if { } ... or whatevs
                return expr;
            }
            else if (Tok is { Kind: TokenKind.While })
            {
                idx++;

                var condition = ParseExpression();
                var expr = new WhileExpression(condition, ParseStatementBodyBraced());
                return expr;
            }

            return ParseAtomExpression();
        }

        private (Token token, OperatorKind kind, Precendence prec, bool rightAssociative)? TryParseOperator()
        {
            if (Tok is Token { SourceValue: string opStr })
            {
                // TODO: There is a better way of writing this.
                var maybeOp = opStr switch
                {
                    "+" => (OperatorKind?)OperatorKind.BinaryPlus,
                    "-" => OperatorKind.BinaryMinus,

                    "*" => OperatorKind.BinaryMultiply,
                    "/" => OperatorKind.BinaryDivide,
                    "%" => OperatorKind.BinaryModulos,

                    "<" => OperatorKind.LessThan,
                    ">" => OperatorKind.GreaterThan,
                    "==" => OperatorKind.Equal,
                    "<=" => OperatorKind.LessThanOrEqual,
                    ">=" => OperatorKind.GreaterThanOrEqual,
                    "!=" => OperatorKind.NotEqual,

                    "." => OperatorKind.Member,
                    "(" => OperatorKind.Call,
                    "[" => OperatorKind.Index,

                    _ => null
                };
                if (maybeOp is not OperatorKind op) return null;

                return (Tok.Value, op, op.GetOperatorPrecedence(), rightAssociative: Tok.Value.Kind == TokenKind.AssignmentSymbol);
            }

            return null;
        }

        private (Token token, OperatorKind kind, Precendence prec)? TryParseUnaryOperator()
        {
            if (Tok is Token { SourceValue: string opStr })
            {
                // TODO: There is a better way of writing this.
                var maybeOp = opStr switch
                {
                    "+" => (OperatorKind?)OperatorKind.UnaryPlus,
                    "-" => OperatorKind.UnaryMinus,
                    "~" => OperatorKind.UnaryBitwiseNegate,
                    "!" => OperatorKind.UnaryLogicalNegate,

                    _ => null
                };
                if (maybeOp is not OperatorKind op) return null;
                
                // purely calling the function just so we make sure these are handled in the switch
                // but the precedence should always just be Unary
                return (Tok.Value, op, op.GetOperatorPrecedence());
            }

            return null;
        }

        private Expression ParseExpressionAtPrecedence(Precendence currentPrecedence)
        {
            // it will always have a value whereever we use it
            Expression expr;
            if (TryParseUnaryOperator() is (_, _, Precendence.Unary) unary)
            {
                idx++;
                expr = new UnaryOperatorExpression(unary.kind, ParseExpressionAtPrecedence(Precendence.Unary));
            }
            else
            {
                expr = ParseSpecialCasesAndAtom();
            }

            var op = TryParseOperator();
            while (op is var (_, kind, precedence, rightAssociative) && (int)precedence < (int)currentPrecedence)
            {
                // we need to move forward to "accept" the op
                idx++;

                // special cases, for call/index we need a comma separated list of args (and there is a terminating symbol)
                // and for member we compress the list down to make it easier to work with i.e. a.b.c rather than a.(b.(c))
                // and it also handles a bunch of ugly syntax that we don't want to support (like parenthesed expressions)
                if (kind == OperatorKind.Call)
                {
                    expr = new CallOperatorExpression(kind, expr, ParseList(Token.Punctuation(")"), Token.Punctuation(","), ParseExpression));
                }
                else if (kind == OperatorKind.Index)
                {
                    expr = new CallOperatorExpression(kind, expr, ParseList(Token.Punctuation("]"), Token.Punctuation(","), ParseExpression));
                }
                else if (kind == OperatorKind.Member)
                {
                    expr = new MemberOperatorExpression(kind, expr, ParseRepetitive(Token.Punctuation("."), ParseAtomExpression, hasFirst: true));
                }
                else
                {
                    var arg = ParseExpressionAtPrecedence(rightAssociative ? precedence : (Precendence)((int)precedence + 1));
                    expr = new BinaryOperatorExpression(kind, expr, arg);
                }

                op = TryParseOperator();
            }

            return expr;
        }

        private Expression ParseExpression()
        {
            // handle some edge cases i.e. functions as expressions, if statement expressions (and other control flows)

            // we always start at 0 and climb up
            return ParseExpressionAtPrecedence(Precendence.MaxPrecendence);
        }

        private List<T> ParseRepetitive<T>(Token next, Func<T> parse, bool hasFirst) where T : ASTNode
        {
            var result = new List<T>() { };
            if (hasFirst) result.Add(parse());

            while (Tok == next)
            {
                idx++;
                result.Add(parse());
            }

            return result;
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
            return new FunctionArg(ConsumeIdent(), TryConsume(Token.Punctuation(":"), ParseType));
        }

        private TypeAst ParseType()
        {
            // types always have an identifier
            // i.e. "int", "Array[int]"
            var ident = ConsumeIdent();
            var args = TryConsume(Token.Punctuation("["), () => ParseList(Token.Punctuation("]"), Token.Punctuation(","), ParseType));

            // for now we'll default construct it to an empty type
            // but maybe?? we can avoid this to save on the allocation
            return new TypeAst(ident, args ?? new());
        }

        private ASTNode ParseStatement()
        {
            // declarations
            if (Tok is { Kind: TokenKind.Const or TokenKind.Var })
            {
                return new VariableDeclaration(
                    isConst: _tokens[idx++].Kind == TokenKind.Const,
                    ConsumeIdent(),
                    TryConsume(Token.Punctuation(":"), ParseType),
                    TryConsume(Token.Assignment("="), ParseExpression));
            }

            if (Tok is { Kind: TokenKind.Return })
            {
                // I think it would be nice to have some concept of an empty return
                // maybe "return void" at worse... or "return null"??
                // or a special keyword like "pass"
                // for now will require a value
                idx++;
                return new ReturnExpression(ParseExpression());
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
