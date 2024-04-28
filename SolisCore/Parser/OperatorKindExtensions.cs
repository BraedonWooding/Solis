using System;

namespace SolisCore.Parser
{
    public static class OperatorKindExtensions
    {
        public static Precendence GetOperatorPrecedence(this OperatorKind op)
        {
            return op switch
            {
                OperatorKind.Call => Precendence.CallIndexMember,
                OperatorKind.Index => Precendence.CallIndexMember,
                OperatorKind.Member => Precendence.CallIndexMember,
                OperatorKind.UnaryPlus => Precendence.Unary,
                OperatorKind.UnaryMinus => Precendence.Unary,
                OperatorKind.UnaryLogicalNegate => Precendence.Unary,
                OperatorKind.UnaryBitwiseNegate => Precendence.Unary,
                OperatorKind.BinaryMultiply => Precendence.BinaryMultiplicative,
                OperatorKind.BinaryDivide => Precendence.BinaryMultiplicative,
                OperatorKind.BinaryModulos => Precendence.BinaryMultiplicative,
                OperatorKind.BinaryPlus => Precendence.BinaryAdditive,
                OperatorKind.BinaryMinus => Precendence.BinaryAdditive,
                OperatorKind.BinaryBitwiseAnd => Precendence.BinaryBitwiseAnd,
                OperatorKind.BinaryBitwiseXor => Precendence.BinaryBitwiseXor,
                OperatorKind.BinaryBitwiseOr => Precendence.BinaryBitwiseOr,
                OperatorKind.LessThan => Precendence.BinaryComparators,
                OperatorKind.GreaterThan => Precendence.BinaryComparators,
                OperatorKind.LessThanOrEqual => Precendence.BinaryComparators,
                OperatorKind.GreaterThanOrEqual => Precendence.BinaryComparators,
                OperatorKind.Equal => Precendence.BinaryComparators,
                OperatorKind.NotEqual => Precendence.BinaryComparators,
                OperatorKind.LogicalAnd => Precendence.LogicalAnd,
                OperatorKind.LogicalOr => Precendence.LogicalOr,
                _ => throw new NotImplementedException(op + " is not yet implemented"),
            };
        }
    }
}
