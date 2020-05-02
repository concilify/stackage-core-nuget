namespace Stackage.Core.Middleware.Options
{
   public class BasePathRewriteRule
   {
      public string Match { get; set; }

      // Proxy adding elements on rewrite (eg. public /foo/bar => private /api/foo/bar)
      public string Added { get; set; }

      // Proxy removing elements on rewrite (eg. public /api/foo/bar => private /foo/bar)
      public string Removed { get; set; }
   }
}
