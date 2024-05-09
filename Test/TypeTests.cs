using SolisCore.Lexing;
using SolisCore.Parser;
using SolisCore.Typechecking;

namespace Test
{
    [TestClass]
    public class TypeTests : VerifyBase
    {
        [TestMethod]
        [DataRow("Declaration Var", "var x: int")]
        [DataRow("Declaration Const", "const y: int")]
        [DataRow("Anonymous Function Variable", "const x = fn(a: int, b: int): float {}")]
        public async Task TestSimpleExpressions(string name, string data)
        {
            var parseTree = Parser.ParseTree(new Lexer().FileToTokens(name, data));
            var typeChecker = new TypeChecker();
            typeChecker.TypeCheckStatement(parseTree);
            await Verify(parseTree).UseMethodName("TestSimpleExpressions_" + name);
        }

        [TestMethod]
        [DynamicData(nameof(TypeVerifyParams), DynamicDataSourceType.Property)]
        public async Task TestVerifyTypes(string name, TypeAst a, TypeAst b)
        {
            var typeChecker = new TypeChecker();
            // just fill it up with a bunch of fresh nodes so we don't need to build
            typeChecker.FreshNodes.AddRange(Enumerable.Range(0, 100).Select(i => new FreshTypeAst(i)));

            await Verify(new
            {
                TypeA = a,
                TypeB = b,
                UnificationResult = typeChecker.UnifyTypes(a, b),
            }).UseMethodName("TestVerifyTypes_" + name);
        }

        public static IEnumerable<object[]> TypeVerifyParams => new List<object[]>
        {
            new object[]{ "Same Types", new TypeAst(Token.Identifier("int"), []), new TypeAst(Token.Identifier("int"), []) },
            new object[]{ "Fresh Types A", new FreshTypeAst(0), new TypeAst(Token.Identifier("int"), []) },
            new object[]{ "Fresh Types B", new TypeAst(Token.Identifier("int"), []), new FreshTypeAst(0) },
            new object[]{ "Both Fresh", new FreshTypeAst(0), new FreshTypeAst(1) },
            new object[]{ "Function Fresh", new FreshTypeAst(0),
                new TypeAst(Token.Identifier("Fn"), [
                    new TypeAst(Token.Identifier("Tuple"), [new(Token.Identifier("int"), []), new(Token.Identifier("float"), [])]),
                    new TypeAst(Token.Identifier("float"), [])
                ])
            },
            new object[]{ "Function Fresh Ish",
                new TypeAst(Token.Identifier("Fn"), [
                    new TypeAst(Token.Identifier("Tuple"), [new FreshTypeAst(0), new(Token.Identifier("float"), [])]),
                    new FreshTypeAst(2)
                ]),
                new TypeAst(Token.Identifier("Fn"), [
                    new TypeAst(Token.Identifier("Tuple"), [new(Token.Identifier("int"), []), new FreshTypeAst(1)]),
                    new TypeAst(Token.Identifier("float"), [])
                ])
            },
        };
    }
}