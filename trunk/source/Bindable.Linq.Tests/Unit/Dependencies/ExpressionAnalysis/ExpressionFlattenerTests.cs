using System;
using System.Linq;
using System.Linq.Expressions;
using Bindable.Linq.Dependencies.ExpressionAnalysis;
using Bindable.Linq.Helpers;
using Bindable.Linq.Tests.MockObjectModel;
using Bindable.Linq.Tests.TestLanguage.Helpers;
using NUnit.Framework;

namespace Bindable.Linq.Tests.Unit.Dependencies.ExpressionAnalysis
{
    /// <summary>
    /// Contains unit tests for the <see cref="ExpressionFlattener"/> class.
    /// </summary>
    [TestFixture]
    public sealed class ExpressionFlattenerTests : TestFixture
    {
        #region Test Helpers
        /// <summary>
        /// Tests the expression.
        /// </summary>
        private static void TestExpression<TDelegate>(Expression<TDelegate> expression, params ExpressionType[] expectedExpressionTypes)
        {
            var flattener = new ExpressionFlattener(expression.Body, ExpressionType.Constant, ExpressionType.Parameter, ExpressionType.MemberAccess);
            var failed = false;
            if (expectedExpressionTypes.Length != flattener.Expressions.Count())
            {
                failed = true;
            }
            else
            {
                for (var i = 0; i < expectedExpressionTypes.Length; i++)
                {
                    if (!flattener.Expressions.Select(x => x.NodeType).Contains(expectedExpressionTypes[i]))
                    {
                        failed = true;
                        break;
                    }
                }
            }
            if (failed)
            {
                Assert.Fail("Expression \"{0}\" should have found expressions \"{1}\", but instead found expressions of type \"{2}\"",
                    expression, expectedExpressionTypes.Select(xt => xt.ToString()).ConcatStrings(", "),
                    flattener.Expressions.Select(x => x.NodeType.ToString()).ConcatStrings(", "));
            }
        }
        #endregion

        /// <summary>
        /// Contains a number of expression tests to exercise various expressions.
        /// </summary>
        [Test]
        public void ExpressionFlattenerCombinedTest()
        {
            var localVariable = new Contact();
            TestExpression<Func<int, int>>(c => c, ExpressionType.Parameter);
            TestExpression<Func<int, int>>(c => c * 27, ExpressionType.Parameter, ExpressionType.Constant);
            TestExpression<Func<int, int>>(c => c * localVariable.Name.Length, ExpressionType.Parameter, ExpressionType.MemberAccess);
            TestExpression<Func<int, int>>(c => c * Math.Min(localVariable.Name.Length, 10), ExpressionType.Parameter, ExpressionType.MemberAccess, ExpressionType.Constant);
            TestExpression<Func<int, int>>(c => c * 21 + Math.Min(localVariable.Name.Length, 10), ExpressionType.Parameter, ExpressionType.Constant, ExpressionType.MemberAccess, ExpressionType.Constant);
            TestExpression<Func<int, int>>(c => c * 21 + Math.Min(localVariable.Name.Length, 10), ExpressionType.Parameter, ExpressionType.Constant, ExpressionType.MemberAccess, ExpressionType.Constant);
            TestExpression<Func<int, int>>(c => c + Math.Min(localVariable.Name.Length, 10).ToString().Length, ExpressionType.Parameter, ExpressionType.MemberAccess);
            TestExpression<Func<int, int>>(c => c > 3 ? localVariable.Name.Length : c, ExpressionType.Parameter, ExpressionType.Constant, ExpressionType.MemberAccess, ExpressionType.Parameter);
            TestExpression<Func<int, string[]>>(c => new[] { c.ToString(), localVariable.Name }, ExpressionType.Parameter, ExpressionType.MemberAccess);
            TestExpression<Func<int, Contact>>(c => new Contact { Name = c.ToString(), Company = localVariable.Name }, ExpressionType.Parameter, ExpressionType.MemberAccess);
            TestExpression<Func<int, int>>(c => 3 + c + 4, ExpressionType.Constant, ExpressionType.Parameter, ExpressionType.Constant);
            TestExpression<Func<object, bool>>(c => c is int, ExpressionType.Parameter);
        }
    }
}