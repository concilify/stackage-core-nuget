using System.Collections.Generic;

namespace Stackage.Core.Middleware.Options
{
   public class BasePathRewritingOptions
   {
      public IList<BasePathRewriteRule> Rules { get; set; }
   }
}
