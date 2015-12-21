using NUnit.Framework;
using System;
using Test.ArithmeticExpression;
using static Racr;

namespace Test {
    [TestFixture]
    public class AttributeTests {
        private Specification _spec;

        [SetUp]
        public void Setup() {
            _spec = new ExpressionSpec();
        }

        [Test]
        public void LambdaAttributeCompilation() {
            _spec.SpecifyAttribute("Eval", "Root", "*", true, (n) => n.GetExp().Eval());
            Assert.DoesNotThrow(() => _spec.CompileAgSpecifications());
        }

        [Test]
        public void StaticMethodAttributeCompilation() {
            _spec.RegisterAgRules(typeof(Attributes));
            Assert.DoesNotThrow(() => _spec.CompileAgSpecifications());
        }

        [Test]
        public void MixedAttributesCompilation() {
            _spec.SpecifyAttribute("Eval", "Root", "*", true, (n) => n.GetExp().Eval());
            _spec.RegisterAgRules(typeof(Attributes));
            Assert.DoesNotThrow(() => _spec.CompileAgSpecifications());
        }

        [Test]
        public void NoVerificationException() {
            _spec.SpecifyAttribute("Eval", "Root", "*", true, (n) => n.GetExp().Eval());
            _spec.SpecifyAttribute("Eval", "Const", "*", true, (node) => node.Lookup(node.GetName()).GetValue());
            _spec.SpecifyAttribute("Eval", "Number", "*", true, (node) => node.GetValue());
            _spec.SpecifyAttribute("Eval", "AddExp", "*", true, (node) => node.GetA().Eval() + node.GetB().Eval());
            _spec.SpecifyAttribute("Eval", "MulExp", "*", true, (node) => node.GetA().Eval() * node.GetB().Eval());
            _spec.RegisterAgRules(typeof(Attributes));
            _spec.CompileAgSpecifications();

            var defs = _spec.CreateAstList(
                _spec.CreateAst("Def", "pi", Math.PI));

            var exp = _spec.CreateAst("AddExp",
                _spec.CreateAst("MulExp",
                    _spec.CreateAst("Number", 1.9098593171027440292266051604702d),
                    _spec.CreateAst("Const", "pi")),
                _spec.CreateAst("MulExp",
                    _spec.CreateAst("Number", 6d),
                    _spec.CreateAst("Number", 6d)));
            var root = _spec.CreateAst("Root", defs, exp);

            Assert.DoesNotThrow(() => root.Eval());
        }
    }
}
