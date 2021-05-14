using System;
using System.Collections.Generic;

namespace Stackage.Core.TypeEnumerators
{
   public interface ITypeEnumerator
   {
      IEnumerable<Type> Types { get; }
   }
}
