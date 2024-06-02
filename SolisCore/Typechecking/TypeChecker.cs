using Microsoft.CodeAnalysis;
using SolisCore.Lexing;
using SolisCore.Parser;
using SolisCore.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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
        public List<TypeAst> FreshNodes { get; } = new();

        public Stack<Scope> Scopes { get; } = new();

        public Dictionary<string, TypeAst> DefinedTypes { get; } = new();

        public TypeChecker()
        {
            DefinedTypes["int"] = new TypeAst(Token.Identifier("int"), new());
            DefinedTypes["float"] = new TypeAst(Token.Identifier("float"), new());
            DefinedTypes["void"] = new TypeAst(Token.Identifier("void"), new());
            DefinedTypes["char"] = new TypeAst(Token.Identifier("char"), new());
            DefinedTypes["string"] = new TypeAst(Token.Identifier("string"), new());
            DefinedTypes["bool"] = new TypeAst(Token.Identifier("bool"), new());
        }

        public TypeAst ResolveFunctionType(FunctionDeclaration decl)
        {
            // we map functions to a generic type that is `Fn[Tuple[...], Ret]`
            var args = new TypeAst(Token.Identifier("Tuple"), decl.Args.Select(arg => ResolveType(arg.TypeAnnotation)).ToList());
            var fnType = new TypeAst(Token.Identifier("Fn"), new() { args, ResolveType(decl.ReturnType) });
            return fnType;
        }

        public TypeAst ResolveIdentifier(string identifier)
        {
            foreach (var scope in Scopes)
            {
                if (scope.Types.TryGetValue(identifier, out var type))
                {
                    return type;
                }
            }

            /* This is where it gets tricky!
             
               Issue is something like this;
               fn foo() { if x { bar() } }
               fn bar() { ... }
              
               bar() is in a top level scope but it could just as easily basically be in any scope
               for *now* what we are going to do is enforce all out of order definitions be in the top scope
               but this isn't realistic for future.
             */
            return Scopes.Last().Types[identifier] = ResolveType(null);
        }

        public TypeAst UnifyTypes(TypeAst typeA, TypeAst? typeB)
        {
            if (typeB == null) return typeA;
            if (typeA == typeB) return typeA;

            static TypeAst AddGenericArgs(TypeAst target, TypeAst from)
            {
                target.GenericArgs.AddRange(from.GenericArgs);
                return target;
            }

            TypeAst ProcessGenericArgs(TypeAst a, TypeAst b)
            {
                // to list just so we can modify freely
                foreach (var ((typeA, typeB), i) in a.GenericArgs.SolisZip(b.GenericArgs).SolisWithIndex().ToList())
                {
                    a.GenericArgs[i] = b.GenericArgs[i] = UnifyTypes(typeA, typeB);
                }

                return a;
            }

            return (typeA, typeB) switch
            {
                (FreshTypeAst freshA, FreshTypeAst freshB) => FreshNodes[freshA.Id] = ResolveType(freshB),
                (FreshTypeAst freshA, TypeAst otherB) => FreshNodes[freshA.Id] = otherB,
                (TypeAst otherA, FreshTypeAst freshB) => FreshNodes[freshB.Id] = otherA,
                (_, _) when typeA.Identifier != typeB.Identifier => throw new Exception("Invalid Unification Bad Type"),
                (_, _) when typeA.GenericArgs.Count == 0 && typeB.GenericArgs.Count > 0
                    => AddGenericArgs(typeA, typeB),
                (_, _) when typeB.GenericArgs.Count == 0 && typeA.GenericArgs.Count > 0
                    => AddGenericArgs(typeB, typeA),
                (_, _) when typeA.GenericArgs.Count != typeB.GenericArgs.Count
                    => throw new Exception("Different generic args count"),
                (_, _) when typeA.GenericArgs.Count == typeB.GenericArgs.Count
                    => ProcessGenericArgs(typeA, typeB),
                _ => throw new NotImplementedException(),
            };
        }

        public TypeAst ResolveType(TypeAst? ast)
        {
            TypeAst type;

            if (ast == null)
            {
                type = new FreshTypeAst(FreshNodes.Count);
                FreshNodes.Add(type);
            }
            else if (ast is FreshTypeAst fresh)
            {
                return FreshNodes[fresh.Id];
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

                type = new TypeAst(ast.Identifier, ast.GenericArgs.Select(ResolveType).ToList());

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

        [return: NotNullIfNotNull(nameof(expr))]
        public TypeAst? TypeCheckExpr(Expression? expr)
        {
            if (expr == null) return null;

            switch (expr)
            {
                case IfExpression ifExpr:
                    {
                        TypeCheckIfExpr(ifExpr);

                        // repeat for the elseifs
                        foreach (var elseIf in ifExpr.ElseIf)
                        {
                            TypeCheckIfExpr(elseIf);
                        }

                        // then else just becomes statement evaluation
                        if (ifExpr.Else != null) TypeCheckStatement(ifExpr.Else, ifExpr);
                        break;
                    }
                case WhileExpression whileExpr:
                    {
                        TypeCheckWhileExpr(whileExpr);
                        break;
                    }
                case FunctionDeclaration decl:
                    {
                        TypeCheckFunction(decl);
                        break;
                    }
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
                            AtomKind.Function => TypeCheckFunction((FunctionDeclaration)atom.Value!),
                            AtomKind.If => TypeCheckIfExpr((IfExpression)atom.Value!),
                            AtomKind.While => TypeCheckWhileExpr((WhileExpression)atom.Value!),
                            AtomKind.Identifier => ResolveIdentifier((string)atom.Value!),
                            _ => throw new NotImplementedException(atom.Kind + " not yet implemented"),
                        };
                        expr.TypeAnnotation = type;
                        break;
                    }
                case MemberOperatorExpression memberOp:
                    {
                        // for now just doing a fresh type, since I don't have proper setups for defined functions
                        memberOp.TypeAnnotation = ResolveType(null);
                        break;
                    }
                case CallOperatorExpression callOp:
                    {
                        var target = TypeCheckExpr(callOp.Target);
                        // we want to unify a function type like this
                        // Fn[A', B'], then extract B'
                        // TODO: This is quite naive and slow, we shold just inspect the type instead...

                        // let's build up the args as types

                        // TODO: Bad nullability here, if it's null we should produce an error or a fresh type??
                        var tuple = new TypeAst(Token.Identifier("Tuple"), callOp.Args.Select(x => TypeCheckExpr(x)!).ToList());
                        var returnFresh = ResolveType(null);
                        var fn = new TypeAst(Token.Identifier("Fn"), new List<TypeAst> { tuple, returnFresh });
                        UnifyTypes(fn, target);

                        // okay now we can access freshB as our "result" type and our args should be unified against the
                        // function type, this actually BAD because it means the function type could change based on callers
                        // which we don't want... *but* we do want to unify anonymous functions based on them being passed into callers
                        // TODO: Fix above
                        callOp.TypeAnnotation = ResolveType(returnFresh);

                        break;
                    }
                case BinaryOperatorExpression binOp:
                    {
                        // right now we won't handle type casting (i.e. int & float = float)
                        var result = UnifyTypes(TypeCheckExpr(binOp.Target), TypeCheckExpr(binOp.Arg));
                        if (binOp.KindGroup == TokenKind.LogicalSymbol)
                        {
                            // both sides need to be boolean for logical compares
                            binOp.TypeAnnotation = UnifyTypes(DefinedTypes["bool"], result);
                        }
                        else if (binOp.KindGroup == TokenKind.ComparatorSymbol)
                        {
                            // results of comparators are always boolean
                            binOp.TypeAnnotation = DefinedTypes["bool"];
                        }
                        else if (binOp.KindGroup == TokenKind.MathSymbol || binOp.KindGroup == TokenKind.BitwiseSymbol)
                        {
                            binOp.TypeAnnotation = result;
                        }
                        else
                        {
                            throw new NotImplementedException("For " + binOp.KindGroup + " " + binOp.Kind);
                        }

                        break;
                    }
                default: throw new NotImplementedException(expr.AstKind + " is not yet implemented");
            }

            // this should be set by the above cases
            return expr.TypeAnnotation!;
        }

        private TypeAst? TypeCheckWhileExpr(WhileExpression whileExpr)
        {
            whileExpr.Condition.TypeAnnotation = UnifyTypes(DefinedTypes["bool"], TypeCheckExpr(whileExpr.Condition));
            // technically the body could result in a value i.e. var x = while x { break 2 }
            TypeCheckStatement(whileExpr.Body, whileExpr);

            return whileExpr.TypeAnnotation;
        }

        public void TypeCheckStatement(StatementBody topLevel, ASTNode? owner = null)
        {
            Scopes.Push(new(owner));

            foreach (var statement in topLevel.Statements)
            {
                switch (statement)
                {
                    case ReturnExpression ret:
                        {
                            // find function scope
                            var fnScope = (FunctionDeclaration?)Scopes.FirstOrDefault(s => s.Owner is FunctionDeclaration)?.Owner
                                ?? throw new Exception("Return not in function scope");
                            fnScope.TypeAnnotation = TypeCheckExpr(ret.Value);
                            break;
                        }
                    case VariableDeclaration decl:
                        {
                            decl.TypeAnnotation = UnifyTypes(ResolveType(decl.TypeAnnotation), TypeCheckExpr(decl.Expression));
                            UpdateType(decl.IdentifierValue, decl.TypeAnnotation);
                            break;
                        }
                    case StatementBody body:
                        {
                            // if we have something like var x = {  }
                            // not sure how we wanna treat that
                            TypeCheckStatement(body, owner: null);
                            break;
                        }
                    case Expression expr:
                        {
                            TypeCheckExpr(expr);
                            break;
                        }
                    default: throw new NotImplementedException(statement.AstKind + " is not implemented");
                }
            }

            Scopes.Pop();
        }

        private TypeAst? TypeCheckIfExpr(IfExpression ifExpr)
        {
            ifExpr.Condition.TypeAnnotation = UnifyTypes(DefinedTypes["bool"], TypeCheckExpr(ifExpr.Condition));
            // technically the body could result in a value i.e. var x = if x { break 2 }
            TypeCheckStatement(ifExpr.Body, ifExpr);

            return ifExpr.TypeAnnotation;
        }

        public void UpdateType(string ident, TypeAst type)
        {
            if (Scopes.Last().Types.TryGetValue(ident, out var definedType))
            {
                // this is an out of order definition
                Scopes.Last().Types[ident] = UnifyTypes(definedType, type);
            }
            else
            {
                // just define in top most scope/first
                Scopes.Peek().Types[ident] = type;
            }
        }

        private TypeAst TypeCheckFunction(FunctionDeclaration decl)
        {
            decl.TypeAnnotation = ResolveFunctionType(decl);
            if (decl.Identifier is { SourceValue: var ident })
            {
                UpdateType(ident, decl.TypeAnnotation);
            }
            TypeCheckStatement(decl.Body, decl);
            return decl.TypeAnnotation;
        }
    }

    public class Scope
    {
        public Scope(ASTNode? owner = null)
        {
            Owner = owner;
        }

        public ASTNode? Owner { get; }

        public Dictionary<string, TypeAst> Types { get; } = new();
    }
}
