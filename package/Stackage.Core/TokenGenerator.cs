using System;
using Stackage.Core.Abstractions;

namespace Stackage.Core
{
   public class TokenGenerator : ITokenGenerator
   {
      public string Generate() => Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
   }
}
