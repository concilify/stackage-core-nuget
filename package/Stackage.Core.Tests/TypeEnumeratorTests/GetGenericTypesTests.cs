using System;
using FluentAssertions;
using NUnit.Framework;
using Stackage.Core.Tests.TestTypes;
using Stackage.Core.TypeEnumerators;

namespace Stackage.Core.Tests.TypeEnumeratorTests
{
   public class GetGenericTypesTests
   {
      private readonly Type[] _availableTypes = new[]
      {
         typeof(DoSomethingWithFoo),
         typeof(DoSomethingWithBar),
         typeof(DoSomethingElseWithFoo),
         typeof(DoSomethingElseWithBar),
         typeof(DoSomethingWithFooAndBar)
      };

      [Test]
      public void returns_five_services_that_implement_do_something_with()
      {
         var discoverFromTypes = new EnumerateTypes(_availableTypes);

         var result = discoverFromTypes.GetGenericTypes(typeof(IDoSomethingWith<>));

         result.Should().BeEquivalentTo(new object[]
         {
            (typeof(DoSomethingWithFoo), new[] {(typeof(IDoSomethingWith<Foo>), new[] {typeof(Foo)})}),
            (typeof(DoSomethingWithBar), new[] {(typeof(IDoSomethingWith<Bar>), new[] {typeof(Bar)})}),
            (typeof(DoSomethingElseWithFoo), new[] {(typeof(IDoSomethingWith<Foo>), new[] {typeof(Foo)})}),
            (typeof(DoSomethingElseWithBar), new[] {(typeof(IDoSomethingWith<Bar>), new[] {typeof(Bar)})}),
            (typeof(DoSomethingWithFooAndBar), new[] {(typeof(IDoSomethingWith<Foo>), new[] {typeof(Foo)}), (typeof(IDoSomethingWith<Bar>), new[] {typeof(Bar)})})
         });
      }

      [Test]
      public void returns_two_services_that_implement_do_something_else_with()
      {
         var discoverFromTypes = new EnumerateTypes(_availableTypes);

         var result = discoverFromTypes.GetGenericTypes(typeof(IDoSomethingElseWith<>));

         result.Should().BeEquivalentTo(new object[]
         {
            (typeof(DoSomethingElseWithFoo), new[] {(typeof(IDoSomethingElseWith<Foo>), new[] {typeof(Foo)})}),
            (typeof(DoSomethingElseWithBar), new[] {(typeof(IDoSomethingElseWith<Bar>), new[] {typeof(Bar)})})
         });
      }

      [Test]
      public void throw_exception_if_type_is_not_generic()
      {
         var discoverFromTypes = new EnumerateTypes(_availableTypes);

         Assert.That(() => discoverFromTypes.GetGenericTypes(typeof(Foo)), Throws.ArgumentException);
      }

      [Test]
      public void throw_exception_if_type_is_generic_with_argument_specified()
      {
         var discoverFromTypes = new EnumerateTypes(_availableTypes);

         Assert.That(() => discoverFromTypes.GetGenericTypes(typeof(IDoSomethingWith<Foo>)), Throws.ArgumentException);
      }
   }
}
