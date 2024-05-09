using SolisCore.Executors;
using SolisCore.Lexing;
using SolisCore.Parser;
using SolisCore.Typechecking;

namespace Test
{
    [TestClass]
    public class ProgramTests : VerifyBase
    {
        [TestMethod]
        [DataRow("Samples/printLine.sol")]
        [DataRow("Samples/factorial.sol")]
        [DataRow("Samples/fibonacci.sol")]
        [DataRow("Samples/helloWorld.sol")]
        public async Task TestPrograms(string fileName)
        {
            var fileContents = File.ReadAllText(fileName);
            var tokens = new Lexer().FileToTokens(fileName, fileContents);
            var ast = Parser.ParseTree(tokens);
            var typechecker = new TypeChecker();
            typechecker.TypeCheckStatement(ast);

            var executor = new ASTExecutor();

            // add global variables for console
            var consoleLogs = new List<string?>();
            executor.Scope.GlobalVariables.Add("std.io.console.printline", (Action<dynamic?>)((object? arg) =>
            {
                consoleLogs.Add(arg?.ToString());
            }));

            executor.Files.Add(fileName, ast);
            executor.ExecuteProgram(fileName);

            await Verify(new
            {
                fileContents,
                tokens,
                ast,
                consoleLogs,
            }).UseMethodName("TestProgram_" + Path.GetFileNameWithoutExtension(fileName));
        }
    }
}