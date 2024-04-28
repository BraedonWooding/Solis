using SolisCore.Lexing;
using SolisCore.Parser;

namespace Test
{
    [TestClass]
    public class ParsingTests : VerifyBase
    {
        [TestMethod]
        [DataRow("Declaration Var", "var x")]
        [DataRow("Declaration Const", "const y")]
        [DataRow("No Args Function", "fn Foo() {}")]
        [DataRow("Anonymous Function Variable", "const x = fn(a, b) {}")]
        [DataRow("Named Function", "fn a(a, b) {}")]
        [DataRow("Simple Math", @"
            const x = a * b + 1
            const y = 1 + a * b")]
        [DataRow("Complex Math", "const x = a + b * 2 + 4 / 9")]
        [DataRow("Unary Math", @"
            const x = -2
            const y = -2 + 3
            const z = -2 * - -+3
        ")]
        public async Task TestSimpleExpressions(string name, string data)
        {
            await Verify(Parser.ParseTree(new Lexer().FileToTokens(name, data))).UseMethodName("TestSimpleExpressions_" + name);
        }
    }
}