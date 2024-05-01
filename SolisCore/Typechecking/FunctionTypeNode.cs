using System.Collections.Generic;

namespace SolisCore.Typechecking
{
    public class FunctionTypeNode : TypeNode
    {
        public FunctionTypeNode(List<TypeNode> args, TypeNode ret)
        {
            Args = args;
            Return = ret;
        }

        public List<TypeNode> Args { get; }
        public TypeNode Return { get; }
    }
}
