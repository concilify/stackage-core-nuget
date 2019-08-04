using System;
using Stackage.Core.Abstractions;

namespace Stackage.Core
{
   public class GuidGenerator : IGuidGenerator
   {
      public string Generate()
      {
         return Guid.NewGuid().ToString();
      }
   }
}
