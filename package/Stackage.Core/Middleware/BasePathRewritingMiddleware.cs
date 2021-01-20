using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Stackage.Core.Middleware.Options;

namespace Stackage.Core.Middleware
{
   public class BasePathRewritingMiddleware
   {
      private readonly RequestDelegate _next;
      private readonly Func<HttpContext, Task> _invokeDelegateAsync;

      public BasePathRewritingMiddleware(RequestDelegate next, IOptions<BasePathRewritingOptions> options)
      {
         if (options == null) throw new ArgumentNullException(nameof(options));

         _next = next ?? throw new ArgumentNullException(nameof(next));

         var basePathRewritingOptions = options.Value;

         if (basePathRewritingOptions.Rules.Length != 0)
         {
            // TODO: Test for this

            if (basePathRewritingOptions.Rules.Any(c => c.Added != null && c.Removed != null))
            {
               throw new ArgumentException("BasePathRewriteRule cannot contain both Added and Removed");
            }

            // TODO: Validate matches in decreasing number of segments

            _invokeDelegateAsync = (context) => InvokeWithBasePathRewriting(context, basePathRewritingOptions);
         }
         else
         {
            _invokeDelegateAsync = (context) => _next(context);
         }
      }

      public async Task Invoke(HttpContext context)
      {
         await _invokeDelegateAsync(context);
      }

      private async Task InvokeWithBasePathRewriting(HttpContext context, BasePathRewritingOptions options)
      {
         foreach (var rule in options.Rules)
         {
            if (context.Request.Path.StartsWithSegments(rule.Match))
            {
               if (rule.Added != null)
               {
                  if (context.Request.Path.StartsWithSegments(rule.Added, out var remainder))
                  {
                     context.Request.Path = remainder;
                  }
               }
               else if (rule.Removed != null)
               {
                  context.Request.PathBase = rule.Removed;
               }

               await _next(context);
               return;
            }
         }

         await _next(context);
      }
   }
}
