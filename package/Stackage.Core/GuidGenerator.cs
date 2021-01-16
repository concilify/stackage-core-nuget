using System;
using Stackage.Core.Abstractions;

namespace Stackage.Core
{
   public class GuidGenerator : IGuidGenerator
   {
      public Guid Generate() => Guid.NewGuid();
   }
}
