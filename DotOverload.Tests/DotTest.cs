using DotOverload;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DotOverload.Tests
{
    [TestClass]
    public class DotTest
    {
        [TestMethod]
        public void Safe_degenerate_case()
        {
            var a = new TestClassA();
            Assert.AreEqual<TestClassA>(Dot.Safe(() => a), a);
        }

        [TestCategory("Bind")]
        [TestMethod]
        public void Bind_degenerate_case()
        {
            Assert.AreEqual(Dot.Bind<object,object>((x,y) => y(x))(() => "hi"), "hi");
        }

        [TestCategory("Bind")]
        [TestMethod]
        public void Bind_does_not_implicitly_change_values()
        {
            var count = 0;
            Func<string> lambda = () => "hi";
            var bind = Dot.Bind<object, string>((c, rest) =>
            {
                if (count == 0)
                {
                    count++;
                }
                else if (count == 1)
                {
                    Assert.AreEqual<object>(c, lambda);
                    count++;
                }
                else if (count == 2)
                {
                    Assert.AreEqual(c, "hi");
                    count++;
                }
                else if (count == 3)
                {
                    Assert.AreEqual(c, 'h');
                    count++;
                }
                else if (count == 4)
                {
                    Assert.AreEqual(c, "h");
                    count++;
                }
                else if (count == 5)
                {
                    Assert.AreEqual(c, "H");
                    count++;
                }
                else if (count == 6)
                {
                    Assert.AreEqual(c, "I");
                    count++;
                }
                return rest(c);
            });

            bind(() => lambda()[0].ToString().ToUpper().Replace('H','I'));
        }

        public void Bind_is_inserted_between_____<T>(Expression<Func<T>> exp, T expected)
        {
            var success = false;
            var insert = Dot.Bind<object, T>((c, rest) =>
            {
                if (expected.Equals(c))
                    success = true;
                return rest(c);
            });

            insert(exp);
            Assert.IsTrue(success);
        }

        [TestCategory("Bind")]
        [TestMethod]
        public void Bind_is_inserted_between_method_calls()
        {
            Bind_is_inserted_between_____(() => "hi".ToUpper(), "HI");
        }

        [TestCategory("Bind")]
        [TestMethod]
        public void Bind_is_inserted_between_member_access()
        {
            var a = new TestClassA();
            a.B = new TestClassB();
            Bind_is_inserted_between_____(() => a.B, a.B);
        }

        [TestCategory("Bind")]
        [TestMethod]
        public void Bind_is_inserted_between_array_index()
        {
            var array = new[] { "hi", "bye" };
            Bind_is_inserted_between_____(() => array[0], array[0]);
        }

        [TestCategory("Bind")]
        [TestMethod]
        public void Bind_is_inserted_between_array_length()
        {
            var array = new[] { "hi", "bye" };
            Bind_is_inserted_between_____(() => array.Length, 2);
        }

        [TestCategory("Bind")]
        [TestMethod]
        public void Bind_is_inserted_between_invoke()
        {
            Func<string> call = () => "hi";
            Bind_is_inserted_between_____(() => call().ToUpper(), "hi");
        }

        [TestCategory("Bind")]
        [TestMethod]
        public void Bind_works_with_binary_operators()
        {
            Assert.AreEqual(0, Dot.Id<object, int>(() => "h"[0].CompareTo('h') + 0));
        }

        [TestCategory("Bind")]
        [TestMethod]
        public void Bind_works_with_list_init()
        {
            Assert.AreEqual(1, Dot.Id<object, int>(() => new List<int>() { 1, 2, 3 }[0]));
        }

        [TestCategory("Bind")]
        [TestMethod]
        public void Bind_works_with_casting()
        {
            Assert.AreEqual(5, Dot.Id<object, int>(() => ((int)((object)5))));
        }

        [TestCategory("Bind")]
        [TestMethod]
        public void Bind_works_with_new()
        {
            Assert.AreEqual(false, Dot.Id<object, bool>(() => new object().Equals(new object())));
        }

        [TestCategory("Bind")]
        [TestMethod]
        public void Bind_is_ran_the_correct_number_of_times()
        {
            var count = 0;
            var counter = Dot.Bind<string, string>((c, rest) =>
            {
                count++;
                return rest(c);
            });

            counter(() => "hi".ToUpper().ToUpper().ToUpper());
            Assert.AreEqual(4, count);
        }

    }

    class TestClassA
    {
        public TestClassB B { get; set; }
        public int Int { get; set; }
    }

    class TestClassB
    {
        public string String { get; set; }
    }
}
