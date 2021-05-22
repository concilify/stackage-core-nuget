using System;
using System.Collections.Generic;

namespace Stackage.Core.TypeEnumerators
{
   public class EnumerateTypesFromAssemblyOf<T> : TypeEnumeratorBase
   {
      public override IEnumerable<Type> Types => typeof(T).Assembly.GetTypes();
   }
}
