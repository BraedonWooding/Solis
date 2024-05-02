using SolisCore.Executors;
using SolisCore.Lexing;
using System.Collections.Generic;

namespace SolisCore.Typechecking
{
    /// <summary>
    /// Used for non-standard types i.e. List[string]
    /// </summary>
    public class VariableTypeNode : TypeNode
    {
        public VariableTypeNode(Token identifier, List<TypeNode> genericArgs)
        {
            Identifier = identifier;
            GenericArgs = genericArgs;
        }

        public Token Identifier { get; }
        public List<TypeNode> GenericArgs { get; }
    }
}
