using Microsoft.CodeAnalysis;
using SolisCore.Executors;
using SolisCore.Lexing;
using SolisCore.Parser;

namespace Test
{
    [TestClass]
    public class ExecutorTests : VerifyBase
    {
        const string Add = @"
            fn Add(a, b) {
                return a + b
            }
        ";

        const string DynamicFunction = @"
            fn GetAdd(a) {
                -- currently it only copies by value but it's okay for now
                return fn(b) {
                    return a + b
                }
            }

            fn Add(a, b) {
                return GetAdd(a)(b)
            }
        ";

        const string Conditional = @"
            fn Equal(a, b) {
                return a == b
            }
        ";

        const string Recursive = @"
            fn RecursiveAdd(a, b) {
                if (b == 0) { return a }
                return RecursiveAdd(a + 1, b - 1)
            }
        ";

        [TestMethod]
        [DataRow("Add", Add, 2, 1, 1)]
        [DataRow("Add", Add, 5, 4, 1)]
        [DataRow("Add", Add, -1, -10, 9)]
        [DataRow("Add", DynamicFunction, -19, -10, -9)]
        [DataRow("Add", DynamicFunction, 50, 0, 50)]
        [DataRow("RecursiveAdd", Recursive, 50, 50, 0)]
        [DataRow("RecursiveAdd", Recursive, 50, 0, 50)]
        [DataRow("Equal", Conditional, true, 0, 0)]
        [DataRow("Equal", Conditional, false, 1, 0)]
        public void TestSimpleExpressions(string name, string data, object result, params object[] args)
        {
            var tree = Parser.ParseTree(new Lexer().FileToTokens(name, data));
            var executor = new ASTExecutor();
            executor.Files.Add(name, tree);
            executor.ExecuteProgram(name);
            Assert.AreEqual(result, executor.RunFunction(name, args));
        }
    }
}