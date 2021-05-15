using System;
using System.Collections.Generic;

namespace Stackage.Core.TypeEnumerators
{
   public class EnumerateTypes : TypeEnumeratorBase
   {
      private readonly Type[] _types;

      public EnumerateTypes(params Type[] types)
      {
         _types = types;
      }

      public override IEnumerable<Type> Types => _types;
   }
}
