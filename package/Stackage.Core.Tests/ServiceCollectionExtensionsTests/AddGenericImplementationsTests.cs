using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Stackage.Core.Extensions;
using Stackage.Core.TypeEnumerators;

namespace Stackage.Core.Tests.ServiceCollectionExtensionsTests
{
   public class AddGenericImplementationsTests
   {
      private readonly Type[] _availableTypes = new[]
      {
         typeof(DoSomethingWithFoo),
         typeof(DoSomethingWithBar),
         typeof(DoSomethingElseWithFoo),
         typeof(DoSomethingElseWithBar)
      };

      [Test]
      public void resolving_for_foo_returns_two_services()
      {
         var services = new ServiceCollection();

         services.AddGenericImplementations(typeof(IDoSomethingWith<>), new EnumerateTypes(_availableTypes), ServiceLifetime.Transient);

         var serviceProvider = services.BuildServiceProvider();

         var result = serviceProvider.GetRequiredService<IEnumerable<IDoSomethingWith<Foo>>>().ToArray();

         Assert.That(result.Count, Is.EqualTo(2));
         Assert.That(result[0].Thing(), Is.EqualTo("Foo1"));
         Assert.That(result[1].Thing(), Is.EqualTo("Foo2"));
      }

      [Test]
      public void resolving_for_bar_returns_two_services()
      {
         var services = new ServiceCollection();

         services.AddGenericImplementations(typeof(IDoSomethingWith<>), new EnumerateTypes(_availableTypes), ServiceLifetime.Transient);

         var serviceProvider = services.BuildServiceProvider();

         var result = serviceProvider.GetRequiredService<IEnumerable<IDoSomethingWith<Bar>>>().ToArray();

         Assert.That(result.Count, Is.EqualTo(2));
         Assert.That(result[0].Thing(), Is.EqualTo("Bar1"));
         Assert.That(result[1].Thing(), Is.EqualTo("Bar2"));
      }

      private interface IDoSomethingWith<T>
      {
         string Thing();
      }

      private class DoSomethingWithFoo : IDoSomethingWith<Foo>
      {
         public string Thing() => "Foo1";
      }
      private class DoSomethingElseWithFoo : IDoSomethingWith<Foo>
      {
         public string Thing() => "Foo2";
      }

      private class DoSomethingWithBar : IDoSomethingWith<Bar>
      {
         public string Thing() => "Bar1";
      }
      private class DoSomethingElseWithBar : IDoSomethingWith<Bar>
      {
         public string Thing() => "Bar2";
      }

      private class Foo
      {
      }

      private class Bar
      {
      }
   }
}
