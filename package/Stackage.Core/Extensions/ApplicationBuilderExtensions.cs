using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Stackage.Core.Middleware;
using Stackage.Core.Middleware.Options;

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

      public static IApplicationBuilder UseDefaultMiddleware(this IApplicationBuilder app, IHostEnvironment environment)
      {
         if (app == null) throw new ArgumentNullException(nameof(app));
         if (environment == null) throw new ArgumentNullException(nameof(environment));

         app.UseMiddleware<BasePathRewritingMiddleware>();

         var options = app.ApplicationServices.GetRequiredService<IOptions<DefaultMiddlewareOptions>>().Value;

         if (options.RunningBehindProxy)
         {
            app.UseForwardedHeaders(new ForwardedHeadersOptions {ForwardedHeaders = ForwardedHeaders.All});
         }
         else if (!environment.IsDevelopment())
         {
            app = app
               .UseHttpsRedirection()
               .UseHsts();
         }

         return app
            .UseMiddleware<TimingMiddleware>()
            .UseMiddleware<RateLimitingMiddleware>()
            .UseMiddleware<ExceptionHandlingMiddleware>()
            .UseMiddleware<PingMiddleware>()
            .UseMiddleware<HealthMiddleware>();
      }
   }
}
