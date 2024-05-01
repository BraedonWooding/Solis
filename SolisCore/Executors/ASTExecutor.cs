using SolisCore.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolisCore.Executors
{
    /// <summary>
    /// Directly executes the AST
    /// 
    /// Isn't meant to be fast but is a way to test compliance of more complicated execution agents
    /// </summary>
    public class ASTExecutor
    {
        public Dictionary<string, StatementBody> Files { get; } = new();

        public Scope Scope = new();

        public dynamic? RunFunction(string functionName, params dynamic?[] args)
        {
            return Eval((FunctionDeclaration)Scope.GlobalVariables[functionName], args);
        }

        private dynamic? Eval(FunctionDeclaration decl, dynamic?[] args)
        {
            // add scope
            var variables = new Dictionary<string, dynamic?>();
            if (args.Length != decl.Args.Count)
            {
                throw new Exception("Too many or too few args");
            }
            for (int i = 0; i < args.Length; i++)
            {
                variables[decl.Args[i].Name.Value] = args[i];
            }
            Scope.Current.Push((decl.Identifier?.Value ?? "Anonymous", variables));
            var result = ExecuteStatementBody(decl.Body);
            var scope = Scope.Current.Pop();
            // todo; validation of scope name

            return result;
        }

        private dynamic? Eval(WhileExpression whileExpr)
        {
            // todo; we should probably allow statements like
            // const x = while y == 2 { break 1 }
            // todo: handle early return
            while ((bool)Eval(whileExpr.Condition))
            {
                ExecuteStatementBody(whileExpr.Body);
            }
            return null;
        }

        private dynamic? Eval(IfExpression ifExpr)
        {
            // todo; we should probably allow statements like
            // const x = if y == 2 { break 1 } else { break 2 }
            // todo: handle early return
            if ((bool)Eval(ifExpr.Condition))
            {
                return ExecuteStatementBody(ifExpr.Body);
            }
            else
            {
                foreach (var elseIfExpr in ifExpr.ElseIf)
                {
                    if ((bool)Eval(elseIfExpr.Condition))
                    {
                        return ExecuteStatementBody(elseIfExpr.Body);
                    }
                }

                if (ifExpr.Else is StatementBody elseBody)
                {
                    return ExecuteStatementBody(elseBody);
                }

                return null;
            }
        }

        private dynamic? Eval(CallOperatorExpression callop)
        {
            if (callop.Kind == OperatorKind.Call)
            {
                var target = Eval(callop.Target)!;
                var args = callop.Args.Select(x => Eval(x)).ToArray();
                if (target is Delegate d)
                {
                    return d.DynamicInvoke(args);
                }
                else if (target is FunctionDeclaration decl)
                {
                    return Eval(decl, args);
                }
                else if (target is IdentifierValue val)
                {
                    throw new Exception("No function for " + val.CurrentPath);
                }
                else
                {
                    throw new Exception($"Invalid Target Type: " + ((object)target).GetType().Name);
                }
            }
            throw new NotImplementedException();
        }

        private dynamic? Eval(Expression expr)
        {
            return expr switch
            {
                MemberOperatorExpression memberop => memberop switch
                {
                    { Kind: OperatorKind.Member, Target: AtomExpression ident } => Scope.LookupVariable(memberop.Path.Select(path => (string)path.Value!).ToList(), ident),
                    { Kind: OperatorKind.Member } => Scope.LookupVariable(memberop.Path.Select(path => (string)path.Value!).ToList(), Eval(memberop.Target)),
                    _ => throw new NotImplementedException(),
                },
                CallOperatorExpression callop => Eval(callop),
                IfExpression ifExpr => Eval(ifExpr),
                WhileExpression whileExpr => Eval(whileExpr),
                BinaryOperatorExpression binop => binop.Kind switch
                {
                    OperatorKind.BinaryMultiply => Eval(binop.Target) * Eval(binop.Arg),
                    OperatorKind.BinaryDivide => Eval(binop.Target) / Eval(binop.Arg),
                    OperatorKind.BinaryModulos => Eval(binop.Target) % Eval(binop.Arg),
                    OperatorKind.BinaryPlus => Eval(binop.Target) + Eval(binop.Arg),
                    OperatorKind.BinaryMinus => Eval(binop.Target) - Eval(binop.Arg),
                    // skipping bitwise for now
                    OperatorKind.BinaryBitwiseAnd => throw new NotImplementedException(),
                    OperatorKind.BinaryBitwiseXor => throw new NotImplementedException(),
                    OperatorKind.BinaryBitwiseOr => throw new NotImplementedException(),
                    OperatorKind.LessThan => (Eval(binop.Target) as IComparable)?.CompareTo(Eval(binop.Arg)) < 0,
                    OperatorKind.GreaterThan => (Eval(binop.Target) as IComparable)?.CompareTo(Eval(binop.Arg)) > 0,
                    OperatorKind.LessThanOrEqual => (Eval(binop.Target) as IComparable)?.CompareTo(Eval(binop.Arg)) <= 0,
                    OperatorKind.GreaterThanOrEqual => (Eval(binop.Target) as IComparable)?.CompareTo(Eval(binop.Arg)) >= 0,
                    OperatorKind.Equal => (Eval(binop.Target) as IComparable)?.CompareTo(Eval(binop.Arg)) == 0,
                    OperatorKind.NotEqual => (Eval(binop.Target) as IComparable)?.CompareTo(Eval(binop.Arg)) != 0,
                    OperatorKind.LogicalAnd => (bool)Eval(binop.Target) || (bool)Eval(binop.Arg),
                    OperatorKind.LogicalOr => (bool)Eval(binop.Target) == (bool)Eval(binop.Arg),
                    _ => throw new NotImplementedException(),
                },
                FunctionDeclaration decl => decl,
                AtomExpression atom => atom.Kind switch
                {
                    AtomKind.Identifier => Scope.LookupVariable(new() { (string)atom.Value! }),
                    AtomKind.ValueInt => atom.Value,
                    AtomKind.ValueFloat => atom.Value,
                    AtomKind.ValueBool => atom.Value,
                    AtomKind.ValueString => atom.Value,
                    AtomKind.ValueChar => atom.Value,
                    AtomKind.ValueNull => atom.Value,
                    _ => throw new NotImplementedException(atom.ToString()),
                },
                _ => throw new Exception("Unhandled " + expr.ToString())
            };
        }

        private dynamic? ExecuteStatementBody(StatementBody body)
        {
            foreach (var statement in body.Statements)
            {
                if (statement is ReturnExpression @return)
                {
                    return Eval(@return.Value);
                }
                else if (statement is FunctionDeclaration decl)
                {
                    Scope.GlobalVariables.Add(decl.Identifier?.Value ?? "Anonymous", decl);
                }
                else if (statement is Expression expr)
                {
                    var res = Eval(expr);
                    if (res != null) return res;
                }
                else if (statement is VariableDeclaration varDecl)
                {
                    Scope.Current.Last().Variables.Add(varDecl.IdentifierValue, varDecl.Expression != null ? Eval((Expression)varDecl.Expression) : null);
                }
                else
                {
                    throw new Exception("Invalid statement " + statement);
                }
            }

            return null;
        }

        public void ExecuteProgram(string program)
        {
            // clear out our scope
            Scope.Current.Clear();

            // push a new scope
            Scope.Current.Push((program, new()));

            ExecuteStatementBody(Files[program]);
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

    public class Scope
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
}
