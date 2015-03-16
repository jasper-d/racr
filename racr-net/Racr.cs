using System;
using IronScheme;
using IronScheme.Runtime;
using IronScheme.Scripting;


static class Racr {

	private static Callable createSpecification;
	private static Callable astRule;
	private static Callable compileAstSpecifications;
	private static Callable compileAgSpecifications;
	private static Callable createAst;
	private static Callable createAstList;
	private static Callable createAstBud;
	private static Callable astParent;
	private static Callable astChild;
	private static Callable astSibling;
	private static Callable astForEachChild;
	private static Callable astFindChild;
	private static Callable astFindChildA;
	private static Callable astNodeQ;
	private static Callable astHasParentQ;
	private static Callable astChildIndex;
	private static Callable astHasChildQ;
	private static Callable astNumChildren;
	private static Callable astHasSiblingQ;
	private static Callable astNodeType;
	private static Callable astListNodeQ;
	private static Callable astBudNodeQ;
	private static Callable astSubtypeQ;


	private static Callable astAnnotationSet;
	private static Callable astWeaveAnnotations;
	private static Callable astAnnotationRemove;
	private static Callable astAnnotationQ;
	private static Callable astAnnotation;

	private static Callable specificationFindAstRule;
	private static Callable astRuleProduction;
	private static Callable symbolIsNonTerminal;


	static Racr() {
		//"(library-path (cons {0} (library-path)))".Eval("../racr-repo/examples");

		"(import (racr core) (racr testing))".Eval();

		// ast
		createSpecification			= "create-specification".Eval<Callable>();
		astRule						= "ast-rule".Eval<Callable>();
		compileAstSpecifications	= "compile-ast-specifications".Eval<Callable>();
		compileAgSpecifications		= "compile-ag-specifications".Eval<Callable>();
		createAst					= "create-ast".Eval<Callable>();
		createAstList				= "create-ast-list".Eval<Callable>();
		createAstBud				= "create-ast-bud".Eval<Callable>();
		astParent					= "ast-parent".Eval<Callable>();
		astChild					= "ast-child".Eval<Callable>();
		astSibling					= "ast-sibling".Eval<Callable>();
		astForEachChild				= "ast-for-each-child".Eval<Callable>();
		astFindChild				= "ast-find-child".Eval<Callable>();
		astFindChildA				= "ast-find-child*".Eval<Callable>();
		astNodeQ					= "ast-node?".Eval<Callable>();
		astHasParentQ				= "ast-has-parent?".Eval<Callable>();
		astChildIndex				= "ast-child-index".Eval<Callable>();
		astHasChildQ				= "ast-has-child?".Eval<Callable>();
		astNumChildren				= "ast-num-children".Eval<Callable>();
		astHasSiblingQ				= "ast-has-sibling?".Eval<Callable>();
		astNodeType					= "ast-node-type".Eval<Callable>();
		astListNodeQ				= "ast-list-node?".Eval<Callable>();
		astBudNodeQ					= "ast-bud-node?".Eval<Callable>();
		astSubtypeQ					= "ast-subtype?".Eval<Callable>();

		// ast annotations
		astAnnotationSet			= "ast-annotation-set!".Eval<Callable>();
		astWeaveAnnotations			= "ast-weave-annotations".Eval<Callable>();
		astAnnotationRemove			= "ast-annotation-remove!".Eval<Callable>();
		astAnnotationQ				= "ast-annotation?".Eval<Callable>();
		astAnnotation				= "ast-annotation".Eval<Callable>();

		// query interface
		specificationFindAstRule	= "specification->find-ast-rule".Eval<Callable>();
		astRuleProduction			= "ast-rule->production".Eval<Callable>();
		symbolIsNonTerminal			= "symbol->non-terminal?".Eval<Callable>();
	}


	public class Specification {
		internal object spec;

		public Specification() {
			spec = createSpecification.Call();
		}
		public void AstRule(string rule) {
			astRule.Call(spec, SymbolTable.StringToObject(rule));
		}
		public void CompileAstSpecifications(string startSymbol) {
			compileAstSpecifications.Call(spec, SymbolTable.StringToObject(startSymbol));
		}
		public void CompileAgSpecifications() {
			compileAgSpecifications.Call(spec);
		}
	}

	private static bool IsTrue(object o) {
		return (o is bool) ? (bool) o : true;
	}

	public struct Range {
		public int min;
		public int max;
		public Range(int min, int max) {
			this.min = min;
			this.max = max;
		}
		public Range(int min) {
			this.min = min;
			this.max = 0;
		}
		internal Cons ToCons() {
			return new Cons(min, max > 0 ? max : SymbolTable.StringToObject("*"));
		}
	}

	public class AstNode {
		internal object ast;
		private bool[] nonTermChilren;		// are children non-terminal?

		private static AstNode GetNode(object ast) {
			return (AstNode) astAnnotation.Call(ast, SymbolTable.StringToObject("this"));
		}
		protected AstNode() {
		}
		public AstNode(Specification spec, string nonTerminal, params object[] children) {

			var nt = SymbolTable.StringToObject(nonTerminal);
			var rule = specificationFindAstRule.Call(spec.spec, nt);
			var symbols = astRuleProduction.Call(rule) as Cons;

			nonTermChilren = new bool[children.Length];

			Cons list = null;
			Cons marker = null;
			for (int i = 0; i < children.Length; i++) {
				symbols = symbols.cdr as Cons;
				nonTermChilren[i] = IsTrue(symbolIsNonTerminal.Call(symbols.car));
				var child = nonTermChilren[i] ? (children[i] as AstNode).ast : children[i];
				var cons = new Cons(child);
				if (list == null) list = marker = cons;
				else {
					marker.cdr = cons;
					marker = cons;
				}
			}
			ast = createAst.Call(spec.spec, nt, list);

			SetAnnotation("this", this);
		}

		public AstNode Parent() {
			return GetNode(astParent.Call(ast));
		}

		// non-terminal
		public AstNode Child(int index) {
			return GetNode(astChild.Call(index, ast));
		}
		public AstNode Child(string name) {
			return GetNode(astChild.Call(SymbolTable.StringToObject(name), ast));
		}
		public AstNode Sibling(int index) {
			return GetNode(astSibling.Call(index, ast));
		}
		public AstNode Sibling(string name) {
			return GetNode(astSibling.Call(SymbolTable.StringToObject(name), ast));
		}


		// terminal
		public T Child<T>(int index) {
			return (T) astChild.Call(index, ast);
		}
		public T Child<T>(string name) {
			return (T) astChild.Call(SymbolTable.StringToObject(name), ast);
		}
		public T Sibling<T>(int index) {
			return (T) astSibling.Call(index, ast);
		}
		public T Sibling<T>(string name) {
			return (T) astSibling.Call(SymbolTable.StringToObject(name), ast);
		}


		public bool IsNode() {
			return (bool) astNodeQ.Call(ast);
		}
		public bool HasParent() {
			object ret = astHasParentQ.Call(ast);
			return IsTrue(ret);
		}
		public int ChildIndex() {
			return (int) astChildIndex.Call(ast);
		}
		public bool HasChild(string name) {
			return (bool) astHasChildQ.Call(SymbolTable.StringToObject(name), ast);
		}
		public int NumChildren() {
			return (int) astNumChildren.Call(ast);
		}
		public bool HasSibling(string name) {
			return (bool) astHasSiblingQ.Call(SymbolTable.StringToObject(name), ast);
		}
		public string NodeType() {
			return ((SymbolId) astNodeType.Call(ast)).ToString();
		}
		public bool IsListNode() {
			return (bool) astListNodeQ.Call(ast);
		}
		public bool IsBudNode() {
			return (bool) astBudNodeQ.Call(ast);
		}


		public void ForEachChild(Action<int, object> f, params Range[] bounds) {
			Func<int, object, object> wrap = (i, n) => {
				f(i, nonTermChilren[i - 1] ? GetNode(n) : n);
				return Builtins.Unspecified;
			};
			object[] l = new object[2 + bounds.Length];
			l[0] = wrap.ToSchemeProcedure();
			l[1] = ast;
			for (int i = 0; i < bounds.Length; i++) l[i + 2] = bounds[i].ToCons();
			astForEachChild.Call(l);
		}
		public object FindChild(Func<int, object, bool> f, params Range[] bounds) {
			int index = 0;
			Func<int, object, bool> wrap = (i, n) => {
				bool found = f(i, nonTermChilren[i - 1] ? GetNode(n) : n);
				if (found) index = i;
				return found;
			};
			object[] l = new object[2 + bounds.Length];
			l[0] = wrap.ToSchemeProcedure();
			l[1] = ast;
			for (int i = 0; i < bounds.Length; i++) l[i + 2] = bounds[i].ToCons();
			var ret = astFindChild.Call(l);
			return (index > 0 && nonTermChilren[index - 1]) ? GetNode(ret) : ret;
		}
		public object FindChildA(Func<int, object, object> f, params Range[] bounds) {
			Func<int, object, object> wrap = (i, n) => {
				return f(i, nonTermChilren[i - 1] ? GetNode(n) : n);
			};
			object[] l = new object[2 + bounds.Length];
			l[0] = wrap.ToSchemeProcedure();
			l[1] = ast;
			for (int i = 0; i < bounds.Length; i++) l[i + 2] = bounds[i].ToCons();
			return astFindChildA.Call(l);
		}



		// ast annotations
		public void SetAnnotation(string name, object v) {
			astAnnotationSet.Call(ast, SymbolTable.StringToObject(name), v);
		}
		public object GetAnnotation(string name) {
			return astAnnotation.Call(ast, SymbolTable.StringToObject(name));
		}
		public void RemoveAnnotation(string name) {
			astAnnotationRemove.Call(ast, SymbolTable.StringToObject(name));
		}
		public bool IsAnnotation(string name) {
			return (bool) astAnnotationQ.Call(ast, SymbolTable.StringToObject(name));
		}
		public void WeaveAnnotations(string type, string name, object v) {
			astWeaveAnnotations.Call(ast, SymbolTable.StringToObject(type), SymbolTable.StringToObject(name), v);
		}
	}

	public static bool IsSubtype(this string t, AstNode n) {
		return (bool) astSubtypeQ.Call(SymbolTable.StringToObject(t), n.ast);
	}
	public static bool IsSubtype(this AstNode n, string t) {
		return (bool) astSubtypeQ.Call(n.ast, SymbolTable.StringToObject(t));
	}
	public static bool IsSubtype(this AstNode n1, AstNode n2) {
		return (bool) astSubtypeQ.Call(n1.ast, n2.ast);
	}


	public class AstList : AstNode {
		public AstList(params AstNode[] children) {
			Cons list = null;
			for (int i = children.Length - 1; i >= 0; i--) list = new Cons(children[i].ast, list);
			ast = createAstList.Call(list);
			SetAnnotation("this", this);
		}
	}

	public class AstBud : AstNode {
		public AstBud() {
			ast = createAstBud.Call();
			SetAnnotation("this", this);
		}
	}
}


class App {


	public static void Main() {


		var spec = new Racr.Specification();
		spec.AstRule("A->B-C-w");
		spec.AstRule("B->t");
		spec.AstRule("C->");

//		spec.AstRule("A->D");
//		spec.AstRule("B:A->");
//		spec.AstRule("C:A->A-A*-t-B-B*");
//		spec.AstRule("D->");

		spec.CompileAstSpecifications("A");
		spec.CompileAgSpecifications();
		var root = new Racr.AstNode(spec, "A",
			new Racr.AstNode(spec, "B",
				"terminal"),
			new Racr.AstNode(spec, "C"),
			"hiya");


		root.ForEachChild((i, o) => {
			var node = o as Racr.AstNode;
			if (node != null)
				Console.WriteLine("{0}: {1}", i, node.NodeType());
			else
				Console.WriteLine("{0}: {1}", i, o);
		}, new Racr.Range(2));


		Console.WriteLine("---");


		var c = root.FindChild((i, o) => {
			return i == 2;
		}) as Racr.AstNode;

		Console.WriteLine("{0}", c.NodeType());


		Console.WriteLine("---");

		Console.WriteLine(root);
		Console.WriteLine("NodeType: {0}", root.NodeType());
		Console.WriteLine("IsNode: {0}", root.IsNode());
		Console.WriteLine("HasParent: {0}", root.HasParent());
		Console.WriteLine("NumChildren: {0}", root.NumChildren());
		Console.WriteLine("HasChild 'B: {0}", root.HasChild("B"));
		Console.WriteLine("HasChild 'Foo: {0}", root.HasChild("Foo"));

		Console.WriteLine("");

		var child = root.Child(1);

		Console.WriteLine(child);
		Console.WriteLine("Child 't: {0}", child.Child<string>("t"));
		Console.WriteLine("IsNode: {0}", child.IsNode());
		Console.WriteLine("HasParent: {0}", child.HasParent());
		Console.WriteLine("ChildIndex: {0}", child.ChildIndex());
		Console.WriteLine("NumChildren: {0}", child.NumChildren());

	}

}