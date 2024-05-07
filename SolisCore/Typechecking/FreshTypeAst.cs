using SolisCore.Lexing;

namespace SolisCore.Typechecking
{
    /// <summary>
    /// A freshly instantiated type that is used to unify types
    /// </summary>
    public class FreshTypeAst : TypeAst
    {
        /// <summary>
        /// File scoped "unique" id for this node
        /// </summary>
        public int Id { get; }

        public FreshTypeAst(int id) : base(Token.Identifier(id.ToString()), new())
        {
            Id = id;
        }
    }
}
