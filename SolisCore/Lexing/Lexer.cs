﻿using SolisCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var file = new FileInfo(fileName, fileContents);

            int currentIdx = 0;
            var span = fileContents.AsSpan();
            while (currentIdx < fileContents.Length)
            {
                // skip ws
                while (span[currentIdx] == '\n' || span[currentIdx] == '\r' || span[currentIdx] == ' ' || span[currentIdx] == '\t')
                {
                    currentIdx++;
                    if (currentIdx >= span.Length)
                    {
                        // early exit incase section ends with a bunch of newlines
                        return result;
                    }
                }

                var idx = 0;
                var token = ParseToken(file, span[currentIdx..], ref idx, currentIdx);
                currentIdx += idx;
                result.Add(token);
            }

            return result;
        }

        public Token ParseToken(FileInfo fileName, ReadOnlySpan<char> span, ref int relativeIdx, int currentIdx)
        {
            if (span.Length == 0)
            {
                throw new Exception("Require at-least one item in span");
            }

            TokenKind kind;
            
            var realValue = (object?)null;
            if (span.StartsWith("--[["))
            {
                // comment block, skip till --]]
                var end = span.IndexOf("--]]");
                if (end == -1)
                {
                    throw new Exception("Error: ");
                }
                relativeIdx += end + "--]]".Length;
                kind = TokenKind.Comment;
            }
            else if (span.StartsWith("--"))
            {
                // skip till '\n'
                var end = span.IndexOf("\n");
                if (end == -1)
                {
                    relativeIdx = span.Length;
                }
                else
                {
                    relativeIdx += end + "\n".Length;
                }
                kind = TokenKind.Comment;
            }
            else if (span.Length >= 2 && TwoCharTokens.TryGetValue(span[0..2].ToString(), out kind))
            {
                relativeIdx += 2;
            }
            else if (span.StartsWith("null"))
            {
                relativeIdx += "null".Length;
                kind = TokenKind.ValueNull;
                realValue = null;
            }
            else if (span.StartsWith("true"))
            {
                relativeIdx += "true".Length;
                kind = TokenKind.ValueBool;
                realValue = true;
            }
            else if (span.StartsWith("false"))
            {
                relativeIdx += "false".Length;
                kind = TokenKind.ValueBool;
                realValue = false;
            }
            else if (span.StartsWith("\""))
            {
                if (span.Length == 1)
                {
                    // then we won't have a terminating \"
                    throw new Exception("Error: missing \"");
                }

                // TODO: Escapes
                var end = span[1..].IndexOf("\"");
                if (end == -1)
                {
                    throw new Exception("Error: missing \"");
                }
                // because the span above for end is [1..] we are off by one and need to increment end
                realValue = span[(relativeIdx + 1)..(end + 1)].ToString();
                relativeIdx += end + "\"".Length * 2;
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

                // for now will keep it as a string but probably should be a char???
                realValue = span[(relativeIdx + 1)..end].ToString();
                relativeIdx += end + "'".Length * 2;
                kind = TokenKind.ValueChar;
            }
            else if (SingleCharTokens.TryGetValue(span[0], out kind))
            {
                relativeIdx += 1;
            }
            else
            {
                switch (span[0])
                {
                    case '.':
                        if (span.Length == 1 || span[1] < '0' || span[1] > '9')
                        {
                            // is just a dot
                            kind = TokenKind.PunctuationSymbol;
                            relativeIdx++;
                        }
                        else
                        {
                            ParseNumeric(span, ref relativeIdx, out kind, out realValue);
                        }

                        break;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        ParseNumeric(span, ref relativeIdx, out kind, out realValue);
                        break;
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'I':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'O':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                    case '_':
                        kind = TokenKind.Identifier;
                        // TODO: Clean up this ugly statement
                        while (relativeIdx < span.Length && ((char.ToLower(span[relativeIdx]) >= 'a' && char.ToLower(span[relativeIdx]) <= 'z') ||
                            (span[relativeIdx] >= '0' && span[relativeIdx] <= '9') || span[relativeIdx] == '_'))
                        {
                            relativeIdx++;
                        }

                        break;
                    default:
                        throw new Exception("Unexpected rest of string: " + span[relativeIdx..].ToString());
                }
            }

            var value = span[..relativeIdx].ToString();
            if (kind == TokenKind.Identifier && ReservedIdentifiers.FirstOrDefault(x => value == x.ToString().ToLowerInvariant()) is var reservedKind && reservedKind != TokenKind.Unknown)
            {
                kind = reservedKind;
            }

            return new Token(kind, value, realValue, new(fileName, currentIdx, relativeIdx));
        }

        public void ParseNumeric(ReadOnlySpan<char> span, ref int currentIdx, out TokenKind kind, out object realValue)
        {
            // note: not supporting hex/binary right now, just simple ints
            // also not supporting underscores in numbers
            SimpleIntParse(span, ref currentIdx);

            if (span.Length > currentIdx && (span[currentIdx] == '.' || char.ToLower(span[currentIdx]) == 'e'))
            {
                kind = TokenKind.ValueFloat;

                if (span[currentIdx] == '.')
                {
                    currentIdx++;

                    // for fractional
                    if (span[currentIdx] >= '0' && span[currentIdx] <= '9')
                    {
                        SimpleIntParse(span, ref currentIdx);
                    }
                    else
                    {
                        throw new Exception("Float parsing '.' should always have a numeric suffix i.e. .5 not .e5");
                    }
                }

                if (span.Length > currentIdx && char.ToLower(span[currentIdx]) == 'e')
                {
                    currentIdx++;
                    if (span[currentIdx] == '+' || span[currentIdx] == '-')
                    {
                        // optional sign
                        currentIdx++;
                    }

                    if (span.Length > currentIdx && span[currentIdx] >= '0' && span[currentIdx] <= '9')
                    {
                        SimpleIntParse(span, ref currentIdx);
                    }
                    else
                    {
                        throw new Exception("Float parsing 'e' should always have a numeric suffix i.e. 1e5 not just 1e+ or something");
                    }
                }

                realValue = float.Parse(span[..currentIdx]);
            }
            else
            {
                kind = TokenKind.ValueInt;
                realValue = int.Parse(span[..currentIdx]);
            }
        }

        public void SimpleIntParse(ReadOnlySpan<char> span, ref int currentIdx)
        {
            while (span.Length > currentIdx && span[currentIdx] >= '0' && span[currentIdx] <= '9')
            {
                currentIdx++;
            }
        }

        public Dictionary<char, TokenKind> SingleCharTokens = new Dictionary<char, TokenKind>
        {
            ['='] = TokenKind.AssignmentSymbol,

            ['+'] = TokenKind.MathSymbol,
            ['-'] = TokenKind.MathSymbol,
            ['*'] = TokenKind.MathSymbol,
            ['/'] = TokenKind.MathSymbol,
            ['%'] = TokenKind.MathSymbol,

            [':'] = TokenKind.PunctuationSymbol,
            ['('] = TokenKind.PunctuationSymbol,
            ['['] = TokenKind.PunctuationSymbol,
            [')'] = TokenKind.PunctuationSymbol,
            [']'] = TokenKind.PunctuationSymbol,
            ['{'] = TokenKind.PunctuationSymbol,
            ['}'] = TokenKind.PunctuationSymbol,
            [','] = TokenKind.PunctuationSymbol,

            ['<'] = TokenKind.ComparatorSymbol,
            ['>'] = TokenKind.ComparatorSymbol,

            ['!'] = TokenKind.BitwiseSymbol,
            ['&'] = TokenKind.BitwiseSymbol,
            ['|'] = TokenKind.BitwiseSymbol,
            ['~'] = TokenKind.BitwiseSymbol,
        };

        public Dictionary<string, TokenKind> TwoCharTokens = new Dictionary<string, TokenKind>
        {
            ["+="] = TokenKind.AssignmentSymbol,
            ["-="] = TokenKind.AssignmentSymbol,
            ["*="] = TokenKind.AssignmentSymbol,
            ["/="] = TokenKind.AssignmentSymbol,
            ["%="] = TokenKind.AssignmentSymbol,

            ["<="] = TokenKind.ComparatorSymbol,
            [">="] = TokenKind.ComparatorSymbol,
            ["=="] = TokenKind.ComparatorSymbol,
            ["!="] = TokenKind.ComparatorSymbol,

            ["&&"] = TokenKind.LogicalSymbol,
            ["||"] = TokenKind.LogicalSymbol,
        };

        public HashSet<TokenKind> ReservedIdentifiers = new HashSet<TokenKind>()
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
