namespace SolisCore.Lexing
{
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
        BitwiseSymbol,
        // i.e. < == != > >= ...
        ComparatorSymbol,
        // i.e. . ( [ { ,
        PunctuationSymbol,
        // i.e. &&, ||
        LogicalSymbol,

        // i.e. --[[ ... --]]
        // and --
        Comment,

        Identifier,

        ValueInt,
        ValueFloat,
        ValueBool,
        ValueString,
        ValueChar,
        ValueNull,
    }
}
