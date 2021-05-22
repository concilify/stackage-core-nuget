namespace Stackage.Core.Tests.TestTypes
{
   public class DoSomethingWithFoo : IDoSomethingWith<Foo>
   {
      public string Thing() => "Foo1";
   }
}
