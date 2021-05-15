using System;
using System.Collections.Generic;
using System.Linq;
using Stackage.Core.TypeEnumerators;

namespace Stackage.Core.Extensions
{
   public static class TypeEnumeratorExtensions
   {
      public static IEnumerable<(Type ImplementationType, (Type ServiceType, Type[] GenericArguments)[] Services)> GetGenericImplementations(
         this ITypeEnumerator discoverFromTypes,
         Type genericServiceType)
      {
         if (!genericServiceType.IsGenericTypeDefinition)
         {
            throw new ArgumentException($"{nameof(genericServiceType)} must be a generic type definition");
         }

         return discoverFromTypes.Types
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Select(t => (t, t.GetInterfaces()
               .Where(c => c.IsGenericType && c.GetGenericTypeDefinition() == genericServiceType)
               .Select(c => (c, c.GetGenericArguments()))
               .ToArray()))
            .Where(c => c.Item2.Length != 0);
      }
   }
}
