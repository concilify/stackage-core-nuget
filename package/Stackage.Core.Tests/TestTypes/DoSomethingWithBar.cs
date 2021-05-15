namespace Stackage.Core.Tests.TestTypes
{
   public class DoSomethingWithBar : IDoSomethingWith<Bar>
   {
      public string Thing() => "Bar1";
   }
}