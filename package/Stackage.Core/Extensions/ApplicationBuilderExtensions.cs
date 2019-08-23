using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Stackage.Core.Middleware;

namespace Stackage.Core.Extensions
{
   public static class ApplicationBuilderExtensions
   {
      public static IApplicationBuilder UseMiddleware<TMiddleware>(this IApplicationBuilder app, string path)
      {
         if (app == null) throw new ArgumentNullException(nameof(app));

         return app.MapWhen(
            context => context.Request.Path.StartsWithSegments(path, out var remainder) && !remainder.HasValue,
            builder => builder.UseMiddleware<TMiddleware>()
         );
      }

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
            .UseMiddleware<PingMiddleware>()
            .UseMiddleware<HealthMiddleware>("/health");
      }
   }
}
