using SolisCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SolisCore.Lexing
{
    /// <summary>
    /// Transforms a code file into a series of location tagged tokens.
    /// </summary>
    public class Lexer
    {
        public List<Token> FileToTokens(string fileName, string fileContents)
        {
            var result = new List<Token>();

            int currentIdx = 0;
            var span = fileContents.AsSpan();
            while (currentIdx < fileContents.Length)
            {
                result.Add(ParseToken(fileName, span[currentIdx..], ref currentIdx));
            }

            return result;
        }

        public Token ParseToken(string fileName, ReadOnlySpan<char> span, ref int currentIdx)
        {
            if (span.Length == 0)
            {
                throw new Exception("Require at-least one item in span");
            }

            var idx = currentIdx;
            // 8 chars is the longest token
            // this could be made to be more performant but shrug
            var longest = span[..Math.Min(8, span.Length)].ToString();
            if (SimpleTokens.FirstOrDefault(x => longest.StartsWith(x.ToString().ToLower())) is var kind)
            {
                currentIdx += kind.ToString().Length;
                return new Token(kind, new(fileName, idx, currentIdx - idx));
            }

            var realValue = (object?)null;
            if (span.StartsWith("--[["))
            {
                // comment block, skip till --]]
                var end = span.IndexOf("--]]");
                if (end == -1)
                {
                    throw new Exception("Error: ");
                }
                currentIdx += end + "--]]".Length + "--[[".Length;
                kind = TokenKind.Comment;
            }
            else if (span.StartsWith("--"))
            {
                // skip till '\n'
                currentIdx += span.IndexOf("\n") + "\n".Length + "--".Length;
                kind = TokenKind.Comment;
            }
            else if (SingleCharTokens.TryGetValue(span[0], out kind))
            {
                currentIdx += 1;
            }
            else if (span.Length >= 2 && TwoCharTokens.TryGetValue(span[0..2].ToString(), out kind))
            {
                currentIdx += 2;
            }
            else if (span.StartsWith("true"))
            {
                currentIdx += "true".Length;
                kind = TokenKind.ValueBool;
            }
            else if (span.StartsWith("false"))
            {
                currentIdx += "false".Length;
                kind = TokenKind.ValueBool;
            }
            else if (span.StartsWith("\""))
            {
                // TODO: Escapes
                var end = span.IndexOf("\"");
                if (end == -1)
                {
                    throw new Exception("Error: ");
                }
                currentIdx += end + "\"".Length * 2;
                kind = TokenKind.ValueString;
            }
            else if (span.StartsWith("'"))
            {
                // TODO: Escapes
                var end = span.IndexOf("'");
                if (end == -1)
                {
                    throw new Exception("Error: ");
                }
                currentIdx += end + "'".Length * 2;
                kind = TokenKind.ValueChar;
            }
            else
            {
                // try parsing float/int
                // LATER DO THIS not gonna care rn
            }

            var value = span[..currentIdx].ToString();
            return new Token(kind, value, realValue, new(fileName, idx, currentIdx - idx));
        }

        public Dictionary<char, TokenKind> SingleCharTokens = new Dictionary<char, TokenKind>
        {
            ['='] = TokenKind.AssignmentSymbol,
            
            ['+'] = TokenKind.MathSymbol,
            ['-'] = TokenKind.MathSymbol,
            ['*'] = TokenKind.MathSymbol,
            ['/'] = TokenKind.MathSymbol,
            ['%'] = TokenKind.MathSymbol,

            ['.'] = TokenKind.PunctuationSymbol,
            ['('] = TokenKind.PunctuationSymbol,
            ['['] = TokenKind.PunctuationSymbol,

            ['<'] = TokenKind.Comparators,
            ['>'] = TokenKind.Comparators,

            ['!'] = TokenKind.BoolSymbols,
            ['&'] = TokenKind.BoolSymbols,
            ['|'] = TokenKind.BoolSymbols,
            ['~'] = TokenKind.BoolSymbols,
        };

        public Dictionary<string, TokenKind> TwoCharTokens = new Dictionary<string, TokenKind>
        {
            ["+="] = TokenKind.AssignmentSymbol,
            ["-="] = TokenKind.AssignmentSymbol,
            ["*="] = TokenKind.AssignmentSymbol,
            ["/="] = TokenKind.AssignmentSymbol,
            ["%="] = TokenKind.AssignmentSymbol,

            ["<="] = TokenKind.Comparators,
            [">="] = TokenKind.Comparators,
            ["=="] = TokenKind.Comparators,
            ["!="] = TokenKind.Comparators,
            ["&&"] = TokenKind.Comparators,
            ["||"] = TokenKind.Comparators,
        };

        public HashSet<TokenKind> SimpleTokens = new HashSet<TokenKind>()
        {
            TokenKind.Var,
            TokenKind.Const,
            TokenKind.Record,
            TokenKind.Fn,
            TokenKind.If,
            TokenKind.Else,
            TokenKind.For,
            TokenKind.While,
            TokenKind.Break,
            TokenKind.Return,
            TokenKind.Continue,
            TokenKind.In,
        };
    }
}
