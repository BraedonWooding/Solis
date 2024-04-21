using SolisCore.Utils;

namespace SolisCore.Lexing
{
    public readonly struct Token
    {
        public readonly TokenKind Kind;
        public readonly string Value;
        // Stores the actual raw value in the file i.e. "0xff" (Value) vs 16 (RealValue)
        // for most characters this is null.
        public readonly object? RealValue;
        public readonly FileSpan Span;

        public Token(TokenKind kind, FileSpan span)
        {
            Kind = kind;
            Value = kind.ToString().ToLower();
            RealValue = null;
            Span = span;
        }

        public Token(TokenKind kind, string value, object? realValue, FileSpan span)
        {
            Kind = kind;
            Value = value;
            RealValue = realValue;
            Span = span;
        }
    }

    public enum TokenKind
    {
        Unknown,
        Var,
        Const,
        Record,
        Fn,
        If,
        Else,
        For,
        While,
        Break,
        Return,
        Continue,
        In,

        // i.e. = += -= ...
        AssignmentSymbol,
        // i.e. + - * ...
        MathSymbol,
        // i.e. ! ~ | &
        BoolSymbols,
        // i.e. < == != > >= ...
        Comparators,
        // i.e. . ( [ {
        PunctuationSymbol,

        // i.e. --[[ ... --]]
        // and --
        Comment,

        // Technically
        ValueInt,
        ValueFloat,
        ValueBool,
        ValueString,
        ValueChar,

    }
}
