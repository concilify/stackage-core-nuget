using System;
using System.Collections.Generic;
using System.Linq;
using Stackage.Core.Abstractions;

namespace Stackage.Core.TypeEnumerators
{
   public abstract class TypeEnumeratorBase : ITypeEnumerator
   {
      public abstract IEnumerable<Type> Types { get; }

      public IEnumerable<(Type ImplementationType, (Type ServiceType, Type[] GenericArguments)[] Services)> GetGenericTypes(Type genericServiceType)
      {
         if (!genericServiceType.IsGenericTypeDefinition)
         {
            throw new ArgumentException($"{nameof(genericServiceType)} must be a generic type definition");
         }

         return Types
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Select(t => (t, t.GetInterfaces()
               .Where(c => c.IsGenericType && c.GetGenericTypeDefinition() == genericServiceType)
               .Select(c => (c, c.GetGenericArguments()))
               .ToArray()))
            .Where(c => c.Item2.Length != 0);
      }
   }
}
