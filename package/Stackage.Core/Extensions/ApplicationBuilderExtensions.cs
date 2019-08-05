using System;
using Microsoft.AspNetCore.Builder;
using Stackage.Core.Middleware;

namespace Stackage.Core.Extensions
{
   public static class ApplicationBuilderExtensions
   {
      public static IApplicationBuilder UseDefaultMiddleware(this IApplicationBuilder app)
      {
         if (app == null) throw new ArgumentNullException(nameof(app));

         return app
            .UseMiddleware<TimingMiddleware>()
            .UseMiddleware<RateLimitingMiddleware>()
            .UseMiddleware<ExceptionHandlingMiddleware>()
            .UseMiddleware<PingMiddleware>();
      }
   }
}
