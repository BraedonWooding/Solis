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
        public async Task TestSimpleExpressions(string name, string data)
        {
            await Verify(Parser.ParseTree(new Lexer().FileToTokens(name, data))).UseMethodName("TestSimpleExpressions_" + name);
        }
    }
}