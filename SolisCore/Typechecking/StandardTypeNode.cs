namespace SolisCore.Typechecking
{
    /// <summary>
    /// For performance and convenience this stores commonly defined types
    /// 
    /// Such as number/int/bool/void/...
    /// </summary>
    public class StandardTypeNode : TypeNode
    {
        public StandardTypeNodeKind Kind { get; }

        public StandardTypeNode(StandardTypeNodeKind kind)
        {
            Kind = kind;
        }

        /// <summary>
        /// Note: This should include specific int types too
        /// </summary>
        public enum StandardTypeNodeKind
        {
            Void,
            Int,
            Number,
            Bool,
            String,
            Char
        }
    }
}
