using System;
using System.Collections.Generic;
using System.Linq;

namespace Stackage.Core.TypeEnumerators
{
   public class EnumerateTypesFromCurrentAppDomain : TypeEnumeratorBase
   {
      public override IEnumerable<Type> Types => AppDomain.CurrentDomain.GetAssemblies().SelectMany(c => c.GetTypes());
   }
}
