namespace SolisCore.Parser
{
    public enum Precendence
    {
        CallIndexMember,
        Unary,

        BinaryMultiplicative,
        BinaryAdditive,
        
        BinaryBitwiseAnd,
        BinaryBitwiseXor,
        BinaryBitwiseOr,

        BinaryComparators,
        
        LogicalAnd,
        LogicalOr,

        // last member
        MaxPrecendence,
    }
}
