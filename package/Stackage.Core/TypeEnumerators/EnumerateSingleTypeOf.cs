using System;
using System.Collections.Generic;

namespace Stackage.Core.TypeEnumerators
{
   public class EnumerateSingleTypeOf<T> : ITypeEnumerator
   {
      public IEnumerable<Type> Types => new[] {typeof(T)};
   }
}