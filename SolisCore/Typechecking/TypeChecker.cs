using SolisCore.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Stack<Scope> Scopes { get; } = new();

        public Dictionary<string, DefinedTypeNode> DefinedTypes { get; } = new();

        public TypeChecker() { }

        public TypeNode ResolveType(TypeAst? ast)
        {
            TypeNode type;

            if (ast == null)
            {
                type = new FreshTypeNode(FreshNodes.Count);
                FreshNodes.Add(type);
            }
            else
            {
                // Identifier could be a literal type like a defined type i.e. "int"
                // or it could be a resolved type like for example "map[int]"
                // it could be defined later in the document so we may need a fresh variable
                // but we don't want to just use a simple one, because this can't be "any" type there is a constraint
                // (that is it has to be a "Type")
                // In future when we add proper generic constraints we could theoretically define it as `'a: Type`
                // but I don't see why I would.

                // The tricky part is how to deal with "partial" generic params, for example `Map[string]`, for now we aren't supporting it
                // this is primarily because I don't see the value, but in the future I would be open to something like `Map[string, _]`
                // which would become `Map[string, 'a]`

                // Note: we do the same thing for a function type for example `Foo[bar]()` would have the type `Fn[(), bar]`
                // Note of note: () refers to a syntatic sugar for Tuple, that is () is equivalent to just Tuple[]
                //               this allows arbitrary tuple packs for example Fn[Tuple[int, int], int] would be add(a: int, b: int): int
                //               and can be written simpler as just Fn[(int, int), int]

                type = new VariableTypeNode(ast.Identifier, ast.GenericTypes.Select(ResolveType).ToList());

                // first handle simple case
                if (DefinedTypes.TryGetValue(ast.Identifier.SourceValue, out var definedType))
                {
                    // todo:
                    // we can unify the types
                }
                else if (Scopes.Peek().Types.TryGetValue(ast.Identifier.SourceValue, out var existingFresh))
                {
                    // todo:
                    // we can unify the types
                }
                else
                {
                    Scopes.Peek().Types.Add(ast.Identifier.SourceValue, type);
                }
            }

            return type;
        }

        public void TypeCheckExpr(ASTNode? expr)
        {
            if (expr == null) return;
            if (expr.TypeAnnotation != null) return;

            switch (expr)
            {
                case AtomExpression atom:
                    {
                        var type = atom.Kind switch
                        {
                            // TODO: handle proper scopes (i.e. traverse upwards)
                            // todo: we shouldn't use defined types???  Just make these standard "int"
                            //AtomKind.Identifier => Scopes.Peek().Types[(string)atom.Value!],
                            // todo:
                            AtomKind.ValueInt => DefinedTypes["int"],
                            AtomKind.ValueFloat => DefinedTypes["float"],
                            AtomKind.ValueBool => DefinedTypes["bool"],
                            AtomKind.ValueString => DefinedTypes["string"],
                            AtomKind.ValueChar => DefinedTypes["char"],
                            // not sure what sits here
                            AtomKind.ValueNull => DefinedTypes["void"],
                            AtomKind.Function => throw new NotImplementedException(),
                            AtomKind.If => throw new NotImplementedException(),
                            AtomKind.While => throw new NotImplementedException(),
                            _ => throw new NotImplementedException(atom.Kind + " not yet implemented"),
                        };

                        break;
                    }
                default: throw new NotImplementedException(expr.AstKind + " is not yet implemented");
            }
        }

        public void TypeCheckDocument(StatementBody topLevel)
        {
            Scopes.Push(new());

            foreach (var statement in topLevel.Statements)
            {
                switch (statement)
                {
                    case VariableDeclaration decl:
                        {
                            Scopes.Peek().Types[decl.IdentifierValue] = ResolveType(decl.TypeAnnotation);
                            // if we have a value
                            TypeCheckExpr(decl.Expression);

                            break;
                        }
                    case FunctionDeclaration decl:
                        {

                            break;
                        }
                    default: throw new NotImplementedException(topLevel.AstKind + " is not implemented");
                }
            }

            Scopes.Pop();
        }
    }

    public class Scope
    {
        public Dictionary<string, TypeNode> Types { get; } = new();
    }
}
