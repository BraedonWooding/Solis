using SolisCore.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolisCore.Transpilers
{
    /// <summary>
    /// Converts the AST to a different representation.
    /// 
    /// This has to be relatively generic because we support multiple different targets right now;
    /// - JS
    /// - C#
    /// - IL (CLR)
    ///   This is more "compilation" but it's just a name.
    /// 
    /// We will also target WASM in the future in a few ways (likely):
    /// 1. "Managed" (I believe this is how C# does it right now) where the code is interpreted by a wasm .net runtime
    /// 2. AOT likely using the same IL to AOT that C# has then running that
    /// 
    /// I don't see us doing direct to x86/wasm compilation anytime soon just due to the complexity being very high
    /// and us not getting much value from it.
    /// 
    /// It is likely for us to target other languages at some point, I could see us having a "rust" or "c++" target at some point.
    /// There would be likely pretty major restrictions on it like a GC[T] wrapper type for all objects created by Solis
    /// and something similar like Owned[T] for all objects owned by the parent language.  The issue here will be figuring out
    /// how to handle ownership in a safe way.  For example
    /// ```rust
    /// let myObj = Owned::new(3)
    /// Foo(myObj)
    /// ```
    /// Does Foo now "own" myObj?  Does it then have to be moved to the GC?
    /// If we move all objects to GC then it'll pretty large performance issues, so perhaps the solution is to disable moving
    /// Requiring something like this...
    /// ```rust
    /// let myObj = Owned::new(3)
    /// Foo(myObj.toGC())
    /// ```
    /// This may not make it seem "good" but we can then enable borrowing
    /// ```rust
    /// let myObj = Owned::new(3)
    /// Foo(&myObj)
    /// ```
    /// This would mean that Foo couldn't let myObj escape and would have to have borrowing semantics.
    /// 
    /// This may seem "crazy" but if we compile Solis to Rust, we likely could auto generate simple borrowing cases
    /// and in more complex cases have a `Ref[T]` type where you need to explicitly denote the mechanics
    /// where this type for C# would just not generate anything extra (i.e. a transparent type).
    /// 
    /// The only issue would be for more complicated lifetimes where you might want to tie 2 lifetimes together
    /// or tie the lifetime of an argument to the class.  This isn't technically that complicated because we could
    /// support it through generics for example.
    /// ```solis
    /// class Container {
    ///     // compile time constant to hold lifetime
    ///     comptime _a = Lifetime.Var()
    ///     items: Ref[Array[int]]
    ///     
    ///     create(items: Ref[Array[int]]): Container {
    ///         // this will "run at compiletime" to validate that the lifetime
    ///         // of items inserted matches to the lifetime of _a
    ///         this.items = _a.ref(items)
    ///     }
    ///     
    ///     getItem(idx: int): Ref[int] {
    ///         // skipping bounds checks to simplify
    ///         return items[idx]
    ///     }
    /// }
    /// 
    /// // would we have to manually specify it like this
    /// // or would everything implicitly have a local lifetime?
    /// let items = new Lifetime.Local[Array[int]]
    /// let container = Container.create(items)
    /// 
    /// // items would then "drop" sometime after this
    /// ```
    /// (of course likely with some syntax sugar to clean it up a little).
    /// 
    /// All this gets very confusing because you have the GC / Rust borrowing
    /// </summary>
    public abstract class Transpiler<TState>
    {
        public Dictionary<string, StatementBody> Files { get; } = new();

        public TranspilerScope Scope = new();

        protected abstract void HandleFunction(FunctionDeclaration decl);

        public void Transpile(TState state, FunctionDeclaration decl)
        {
            Scope.Current.Push((decl.Identifier.Value.SourceValue, new()));

            Transpile(state, decl.Body);

            Scope.Current.Pop();
        }

        private void Transpile(TState state, StatementBody body)
        {

        }

        private dynamic? TranspileStatements(StatementBody body)
        {
            foreach (var statement in body.Statements)
            {
                if (statement is ReturnExpression @return)
                {
                    return Transpile(@return.Value);
                }
                else if (statement is FunctionDeclaration decl)
                {
                    Scope.GlobalVariables.Add(decl.Identifier?.SourceValue ?? "Anonymous", decl);
                }
                else if (statement is Expression expr)
                {
                    var res = Transpile(expr);
                    if (res != null) return res;
                }
                else if (statement is VariableDeclaration varDecl)
                {
                    Scope.Current.Last().Variables.Add(varDecl.IdentifierValue, varDecl.Expression != null ? Transpile(varDecl.Expression) : null);
                }
                else
                {
                    throw new Exception("Invalid statement " + statement);
                }
            }

            return null;
        }

        public void AddProgram(string program)
        {
            // clear out our scope
            Scope.Current.Clear();

            // push a new scope
            Scope.Current.Push((program, new()));

            TranspileStatements(Files[program]);

            Scope.Current.Pop();
        }

        public class TranspilerScope
        {
            public Dictionary<string, dynamic?> GlobalVariables { get; } = new();
            public Stack<(string Name, Dictionary<string, dynamic?> Variables)> Current { get; } = new();

            public string ScopeId => string.Join('.', Current.Select(scope => scope.Name));

            public dynamic? LookupVariable(List<string> memberPaths, dynamic? target = null)
            {
                target ??= new IdentifierValue("");
                if (target is AtomExpression expr)
                {
                    target = null;
                    foreach (var (_, Variables) in Current)
                    {
                        if (Variables.TryGetValue(expr.Value!.ToString(), out var varLocal))
                        {
                            target = varLocal;
                            break;
                        }
                    }

                    target ??= GlobalVariables.GetValueOrDefault(expr.Value!.ToString(), new IdentifierValue(expr.Value?.ToString()!));
                }

                if (target is IdentifierValue ident)
                {
                    target = ident = new IdentifierValue(ident.CurrentPath);
                    foreach (var memberPath in memberPaths) ident.Push(memberPath);

                    foreach (var (_, Variables) in Current)
                    {
                        if (Variables.TryGetValue(ident.CurrentPath, out var varLocal))
                        {
                            target = varLocal;
                            break;
                        }
                    }

                    if (GlobalVariables.TryGetValue(ident.CurrentPath, out var varGlobal))
                    {
                        target = varGlobal;
                    }
                }
                else
                {
                    throw new Exception("Current we don't support assignment :(");
                }

                return target;
            }
        }

        public class IdentifierValue
        {
            public string CurrentPath { get; set; }

            public IdentifierValue(string currentPath)
            {
                CurrentPath = currentPath;
            }

            public void Push(string path)
            {
                if (string.IsNullOrEmpty(CurrentPath)) CurrentPath = path;
                else CurrentPath += "." + path;
            }
        }
    }
}
