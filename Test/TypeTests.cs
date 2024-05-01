using SolisCore.Lexing;
using SolisCore.Parser;

namespace Test
{
    [TestClass]
    public class TypeTests : VerifyBase
    {
        [TestMethod]
        [DataRow("Declaration Var", "var x: int")]
        [DataRow("Declaration Const", "const y: int")]
        [DataRow("Anonymous Function Variable", "const x = fn(a: int, b: int) {}")]
        public async Task TestSimpleExpressions(string name, string data)
        {
            await Verify(Parser.ParseTree(new Lexer().FileToTokens(name, data))).UseMethodName("TestSimpleExpressions_" + name);
        }
    }
}