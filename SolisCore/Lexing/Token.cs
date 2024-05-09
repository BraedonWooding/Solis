using SolisCore.Utils;
using System;

namespace SolisCore.Lexing
{
    public readonly struct Token : IEquatable<Token>
    {
        public readonly TokenKind Kind;
        public readonly string SourceValue;
        // Stores the actual raw value in the file i.e. "0xff" (Value) vs 16 (ParsedValue)
        // for most characters this is null.
        public readonly object? ParsedValue;
        public readonly FileSpan Span;

        public Token(TokenKind kind, FileSpan span)
        {
            Kind = kind;
            SourceValue = kind.ToString().ToLower();
            ParsedValue = null;
            Span = span;
        }

        public Token(TokenKind kind, string value, object? realValue, FileSpan span)
        {
            Kind = kind;
            SourceValue = value;
            ParsedValue = realValue;
            Span = span;
        }

        public override string ToString()
        {
            // TODO: There is a better way then just checking bytelength == 0
            return $"Token({Kind}: {SourceValue}){(Span.ByteLength == 0 ? Span.ToString() : "")}";
        }

        public static Token Identifier(string ident)
        {
            return new Token(TokenKind.Identifier, ident, null, new());
        }

        public static Token Punctuation(string symbol)
        {
            return new Token(TokenKind.PunctuationSymbol, symbol, null, new FileSpan());
        }

        public static Token BitwiseMath(string symbol)
        {
            return new Token(TokenKind.BitwiseSymbol, symbol, null, new FileSpan());
        }

        public static Token Math(string symbol)
        {
            return new Token(TokenKind.MathSymbol, symbol, null, new FileSpan());
        }

        public static Token Logical(string symbol)
        {
            return new Token(TokenKind.LogicalSymbol, symbol, null, new FileSpan());
        }

        public static Token Compare(string symbol)
        {
            return new Token(TokenKind.ComparatorSymbol, symbol, null, new FileSpan());
        }

        public static Token Assignment(string symbol)
        {
            return new Token(TokenKind.AssignmentSymbol, symbol, null, new FileSpan());
        }

        public override bool Equals(object? obj)
        {
            return obj is Token token && Equals(token);
        }

        public bool Equals(Token other)
        {
            return Kind == other.Kind &&
                   SourceValue == other.SourceValue;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Kind, SourceValue);
        }

        public static bool operator ==(Token left, Token right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Token left, Token right)
        {
            return !(left == right);
        }
    }
}
