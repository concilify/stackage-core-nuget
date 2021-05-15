using System;

namespace Stackage.Core.Tests.TestTypes
{
   public class DoSomethingElseWithFoo : IDoSomethingWith<Foo>, IDoSomethingElseWith<Foo>
   {
      public string Thing() => "Foo2";

      public string AnotherThing()
      {
         throw new NotSupportedException();
      }
   }
}
