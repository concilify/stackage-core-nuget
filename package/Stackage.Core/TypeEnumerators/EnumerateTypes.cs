using System;
using System.Collections.Generic;

namespace Stackage.Core.TypeEnumerators
{
   public class EnumerateTypes : ITypeEnumerator
   {
      private readonly Type[] _types;

      public EnumerateTypes(params Type[] types)
      {
         _types = types;
      }

      public IEnumerable<Type> Types => _types;
   }
}