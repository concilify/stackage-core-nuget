using Stackage.Core.Abstractions;

namespace Stackage.Core.Extensions
{
   public static class GuidGeneratorExtensions
   {
      public static string GenerateToken(this IGuidGenerator guidGenerator)
      {
         return guidGenerator.Generate().ToString().Substring(0, 8).ToUpper();
      }
   }
}
