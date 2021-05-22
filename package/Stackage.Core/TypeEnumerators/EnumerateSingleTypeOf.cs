using System;
using System.Collections.Generic;

namespace Stackage.Core.TypeEnumerators
{
   public class EnumerateSingleTypeOf<T> : TypeEnumeratorBase
   {
      public override IEnumerable<Type> Types => new[] {typeof(T)};
   }
}
