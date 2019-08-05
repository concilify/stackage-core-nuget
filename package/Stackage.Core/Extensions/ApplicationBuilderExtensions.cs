using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Stackage.Core.Middleware;

namespace Stackage.Core.Extensions
{
   public static class ApplicationBuilderExtensions
   {
      public static IApplicationBuilder UseDefaultMiddleware(this IApplicationBuilder app, IHostingEnvironment environment)
      {
         if (app == null) throw new ArgumentNullException(nameof(app));

         if (!environment.IsDevelopment())
         {
            app = app
               .UseMiddleware<HttpsRedirectionMiddleware>()
               .UseMiddleware<HstsMiddleware>();
         }

         return app
            .UseMiddleware<TimingMiddleware>()
            .UseMiddleware<RateLimitingMiddleware>()
            .UseMiddleware<ExceptionHandlingMiddleware>()
            .UseMiddleware<PingMiddleware>();
      }
   }
}
