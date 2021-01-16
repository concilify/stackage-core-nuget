using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Prometheus;
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

         var stackageOptions = app.ApplicationServices.GetRequiredService<IOptions<StackageOptions>>().Value;

         if (stackageOptions.RunningBehindProxy)
         {
            app.UseForwardedHeaders();
         }
         else if (!environment.IsDevelopment())
         {
            app = app
               .UseHttpsRedirection()
               .UseHsts();
         }

         return app
            .UseMiddleware<LivenessMiddleware>()
            .UseMetricServer("/metrics")
            .UseMiddleware<ReadinessMiddleware>()
            .UseMiddleware<StartupTasksMiddleware>()
            .UseMiddleware<MetricsAndExceptionHandlingMiddleware>()
            .UseMiddleware<RateLimitingMiddleware>()
            .UseMiddleware<HealthMiddleware>();
      }
   }
}
