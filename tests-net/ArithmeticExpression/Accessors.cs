using static Racr;

namespace Test.ArithmeticExpression {
    public static class Accessors {
        public static AstNode Lookup(this AstNode n, string name) {
            return n.AttValue<AstNode>("Lookup", name);
        }
        public static AstNode GetExp(this AstNode n) {
            return n.Child("Exp");
        }
        public static AstNode GetDefs(this AstNode n) {
            return n.Child("Defs");
        }
        public static AstNode GetA(this AstNode n) {
            return n.Child("A");
        }
        public static AstNode GetB(this AstNode n) {
            return n.Child("B");
        }
        public static double GetValue(this AstNode n) {
            return n.Child<double>("value");
        }
        public static string GetName(this AstNode n) {
            return n.Child<string>("name");
        }
        // Attribute
        public static double Eval(this AstNode n) {
            return n.AttValue<double>("Eval");
        }
    }
}
