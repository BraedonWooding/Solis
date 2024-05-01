﻿using SolisCore.Executors;
using System.Collections.Generic;

namespace SolisCore.Typechecking
{
    /// <summary>
    /// Used for non-standard types i.e. List[string]
    /// </summary>
    public class VariableTypeNode : TypeNode
    {
        public VariableTypeNode(IdentifierValue identifier, List<TypeNode> genericArgs)
        {
            Identifier = identifier;
            GenericArgs = genericArgs;
        }

        public IdentifierValue Identifier { get; }
        public List<TypeNode> GenericArgs { get; }
    }
}