using static Racr;

namespace Test.ArithmeticExpression {
    class ExpressionSpec : Specification {
        public ExpressionSpec() {
            AstRule("Root->Def*<Defs-Exp");
            AstRule("Def->name-value");
            AstRule("Exp->");
            AstRule("BinExp:Exp->Exp<A-Exp<B");
            AstRule("AddExp:BinExp->");
            AstRule("MulExp:BinExp->");
            AstRule("Number:Exp->value");
            AstRule("Const:Exp->name");
            CompileAstSpecifications("Root");
        }
    }
}
