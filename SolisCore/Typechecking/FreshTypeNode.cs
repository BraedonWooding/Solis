namespace SolisCore.Typechecking
{
    /// <summary>
    /// A freshly instantiated type that is used to unify types
    /// </summary>
    public class FreshTypeNode : TypeNode
    {
        /// <summary>
        /// File scoped "unique" id for this node
        /// </summary>
        public int Id { get; }

        public FreshTypeNode(int id)
        {
            Id = id;
        }
    }
}
