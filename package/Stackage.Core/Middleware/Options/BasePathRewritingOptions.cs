using System;

namespace Stackage.Core.Middleware.Options
{
   public class BasePathRewritingOptions
   {
      public BasePathRewriteRule[] Rules { get; set; } = Array.Empty<BasePathRewriteRule>();
   }
}
