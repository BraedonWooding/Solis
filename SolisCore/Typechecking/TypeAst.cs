using SolisCore.Lexing;
using System.Collections.Generic;

namespace SolisCore.Typechecking
{
    public class TypeAst
    {
        public TypeAst(Token identifier, List<TypeAst> genericArgs)
        {
            Identifier = identifier;
            GenericArgs = genericArgs;
        }

        public string Type => GetType().Name;

        public Token Identifier { get; }
        public List<TypeAst> GenericArgs { get; }
    }
}
