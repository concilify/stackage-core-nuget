namespace Stackage.Core.Tests.TestTypes
{
   public class DoSomethingWithFooAndBar : IDoSomethingWith<Foo>, IDoSomethingWith<Bar>
   {
      public string Thing() => "FooAndBar";
   }
}
