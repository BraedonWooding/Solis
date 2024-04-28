using SolisCore.Lexing;
using SolisCore.Parser;

namespace Test
{
    [TestClass]
    public class ProgramTests : VerifyBase
    {
        [TestMethod]
        [DataRow("Samples/factorial.sol")]
        [DataRow("Samples/fibonacci.sol")]
        [DataRow("Samples/helloWorld.sol")]
        public async Task TestPrograms(string fileName)
        {
            var fileContents = File.ReadAllText(fileName);
            var tokens = new Lexer().FileToTokens(fileName, fileContents);
            var ast = Parser.ParseTree(tokens);
            await Verify(new
            {
                fileContents,
                tokens,
                ast,
            }).UseMethodName("TestProgram_" + Path.GetFileNameWithoutExtension(fileName));
        }
    }
}