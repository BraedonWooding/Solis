namespace SolisCore.Parser
{
    // Ordered roughly in precedence order
    public enum OperatorKind
    {
        Call,
        Member,
        Index,

        // same prec
        UnaryPlus,
        UnaryMinus,
        UnaryLogicalNegate,
        UnaryBitwiseNegate,

        // same prec
        BinaryMultiply,
        BinaryDivide,
        BinaryModulos,

        // same prec
        BinaryPlus,
        BinaryMinus,

        // or -> xor -> and, prec matters
        BinaryBitwiseAnd,
        BinaryBitwiseXor,
        BinaryBitwiseOr,

        // prec all the same
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
        Equal,
        NotEqual,

        // or -> and, prec matters
        LogicalAnd,
        LogicalOr,
    }
}
