using SolisCore.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace SolisCore.Typechecking
{
    /// <summary>
    /// Is a relatively standard unification typechecker with good inference support
    /// </summary>
    public class TypeChecker
    {
        /// <summary>
        /// Look up your fresh node id in this to find what it's resolved type is.
        /// 
        /// This is a chainable lookup i.e. A -> B -> C -> int or something.
        /// </summary>
        public List<TypeNode> FreshNodes { get; } = new();

        public TypeChecker() { }

        public void TypeCheckDocument(StatementBody topLevel)
        {
            foreach (var statement in topLevel.Statements)
            {

            }
        }
    }

    public class Scope
    {

    }
}
