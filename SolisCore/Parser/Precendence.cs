namespace SolisCore.Parser
{
    public enum Precendence
    {
        CallAndIndex,
        // TODO: Prec on this is weird
        Member,
        // since precedence 
        Unary,
        BinaryAdditive,
        BinaryMultiplicative,
        
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
