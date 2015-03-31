using System;
using IronScheme;
using IronScheme.Runtime;
using NUnit.Framework;

[TestFixture]
class Test {

	Callable load;
	Callable interpretCorrect;
	Callable interpretIncorrect;

	[SetUp]
	public void Init() {
		"(import (racr core) (racr testing))".Eval();

		load = "load".Eval<Callable>();

		// extend library path
		"(library-path (cons {0} (library-path)))".Eval("../racr-repo/examples");

		// siple
		"(import (siple main) (siple exception-api))".Eval();
		interpretCorrect = "siple-interpret".Eval<Callable>();
		interpretIncorrect = @"
		(lambda (x)
		  (assert-exception
		    siple-exception?
			(siple-interpret x)))
		".Eval<Callable>();
	}

	[Test]
	public void SchemeAdd() {
		Assert.AreEqual(3, "(+ 1 2)".Eval<int>());
	}

	[Test]
	public void RacrBasicTests() {
		foreach (var test in new string[] {
			"ast-construction.scm",
			"attribute-evaluation-basics.scm",
			"continuations-in-equations.scm",
			"patterns.scm",
			"rewrite-basics.scm",
			"rewrite-buds.scm",
			"rewrite-lists.scm",
			"rewrite-refine-abstract.scm",
			"rewrite-strategies.scm",
		}) {
			var path = "../racr-repo/tests/" + test;
			Console.WriteLine("loading {0} ...", path);
			load.Call(path);
		}
	}

	[Test]
	public void RacrExamples() {
		foreach (var test in new string[] {
			"binary-numbers/binary-numbers.scm",
			"petrinets/examples/cookie-automata.scm",
			"petrinets/examples/purchase-processing.scm",
			"petrinets/examples/runtime-structure-example-slide.scm",
			"state-machines/state-machines.scm",
		}) {
			var path = "../racr-repo/examples/" + test;
			Console.WriteLine("loading {0} ...", path);
			load.Call(path);
		}
	}

	[Test]
	public void SipleCorrectExamples() {
		foreach (var test in new string[] {
			"abnormal_termination.siple",
			"assertions.siple",
			"boolean_arithmetics.siple",
			"closures.siple",
			"control_flow.siple",
			"integer_and_real_arithmetics.siple",
			"nested_procedures.siple",
			"pointers.siple",
			"procedure_basics.siple",
			"relational_arithmetics.siple",
			"scopes.siple",
			"type_coercions.siple",
		}) {
			var path = "../racr-repo/examples/siple/examples/correct/" + test;
			Console.WriteLine("interpreting {0} ...", path);
			interpretCorrect.Call(path);
		}
	}

	[Test]
	public void SipleIncorrectExamples() {
		foreach (var test in new string[] {
			"abnormal_termination.siple",
			"assertions.siple",
			"boolean_arithmetics.siple",
			"control_flow.siple",
			"integer_and_real_arithmetics.siple",
			"procedure_basics.siple",
			"relational_arithmetics.siple",
			"scopes.siple",
		}) {
			var path = "../racr-repo/examples/siple/examples/incorrect/" + test;
			Console.WriteLine("interpreting {0} ...", path);
			interpretIncorrect.Call(path);
		}
	}
}