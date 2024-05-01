using SolisCore.Lexing;

namespace Test
{
    [TestClass]
    public class LexingTests : VerifyBase
    {
        [TestMethod]
        [DataRow("SimpleInts", "1 2 3 4 5 6 7 8 9 0")]
        [DataRow("CompoundInts", "1234567890")]
        [DataRow("SimpleFloats", "12323.112 0.55 .3 .0111 0.1")]
        [DataRow("ExpFloats", "1e10 .1e24 9.8e5")]
        [DataRow("Ident", "Hello _goodbyte H_goodbye h123_2323dfsdsfdsf _ h _1")]
        [DataRow("Variable Decl", "var x = 2 const x = 3")]
        [DataRow("String", "\"Hello!\"")]
        [DataRow("Multiple String", "\"Hello!\"\"World\"")]
        [DataRow("Reserved words", "if while in")]
        [DataRow("NonReserved words", "ifa whilea ina")]
        public async Task TestSimpleExpressions(string name, string data)
        {
            await Verify(new Lexer().FileToTokens(name, data)).UseMethodName("TestSimpleExpressions_" + name);
        }
    }
}