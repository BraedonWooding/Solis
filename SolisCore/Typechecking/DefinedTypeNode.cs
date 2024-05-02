using SolisCore.Executors;
using SolisCore.Lexing;
using System.Collections.Generic;

namespace SolisCore.Typechecking
{
    /// <summary>
    /// Represents system defined types.
    /// 
    /// For example "int"
    /// </summary>
    public class DefinedTypeNode
    {
        public DefinedTypeNode(string identifier, List<GenericTypeParam>? genericParams = null)
        {
            Identifier = identifier;
            GenericParams = genericParams ?? new();
        }

        public string Identifier { get; }
        public List<GenericTypeParam> GenericParams { get; }
    }

    public class GenericTypeParam
    {
        public string Identifier { get; }

        public GenericTypeParam(string identifier)
        {
            Identifier = identifier;
        }

        // TODO: Constraints
    }
}
