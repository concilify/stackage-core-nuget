using System;
using System.Collections.Generic;
using System.Linq;

namespace Stackage.Core.TypeEnumerators
{
   public class EnumerateTypesFromCurrentAppDomain : ITypeEnumerator
   {
      public IEnumerable<Type> Types => AppDomain.CurrentDomain.GetAssemblies().SelectMany(c => c.GetTypes());
   }
}