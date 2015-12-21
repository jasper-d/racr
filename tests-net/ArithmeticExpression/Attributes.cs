using static Racr;

namespace Test.ArithmeticExpression {
    public class Attributes {
        [AgRule("Lookup", "Root", Cached = true, Context = "*")]
        private static AstNode EvalConst(AstNode node, string name) {
            return (AstNode)node.GetDefs().FindChild((i, d) => ((AstNode)d).GetName() == name);
        }
    }
}
