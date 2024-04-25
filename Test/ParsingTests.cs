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
        [DataRow("Anonymous Function", "fn(a, b) {}")]
        [DataRow("Anonymous Function Variable", "const x = fn(a, b) {}")]
        [DataRow("Named Function", "fn a(a, b) {}")]
        [DataRow("Named Function Variable", "const x = fn b(a, b) {}")]
        public async Task TestSimpleExpressions(string name, string data)
        {
            await Verify(Parser.ParseTree(new Lexer().FileToTokens(name, data))).UseMethodName("TestSimpleExpressions_" + name);
        }
    }
}