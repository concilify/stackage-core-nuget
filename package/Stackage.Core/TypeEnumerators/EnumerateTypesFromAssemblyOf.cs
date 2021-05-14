using System;
using System.Collections.Generic;

namespace Stackage.Core.TypeEnumerators
{
   public class EnumerateTypesFromAssemblyOf<T> : ITypeEnumerator
   {
      public IEnumerable<Type> Types => typeof(T).Assembly.GetTypes();
   }
}