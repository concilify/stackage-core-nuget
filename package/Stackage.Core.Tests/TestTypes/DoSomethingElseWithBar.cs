using System;

namespace Stackage.Core.Tests.TestTypes
{
   public class DoSomethingElseWithBar : IDoSomethingWith<Bar>, IDoSomethingElseWith<Bar>
   {
      public string Thing() => "Bar2";

      public string AnotherThing()
      {
         throw new NotSupportedException();
      }
   }
}
