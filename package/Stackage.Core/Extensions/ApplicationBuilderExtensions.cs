using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Prometheus;
using Stackage.Core.MetricSinks;
using Stackage.Core.Middleware;
using Stackage.Core.Options;

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

      public static IApplicationBuilder UseDefaultMiddleware(this IApplicationBuilder app)
      {
         if (app == null) throw new ArgumentNullException(nameof(app));

         var stackageOptions = app.ApplicationServices.GetRequiredService<IOptions<StackageOptions>>().Value;
         var prometheusOptions = app.ApplicationServices.GetRequiredService<IOptions<PrometheusOptions>>().Value;

         if (stackageOptions.ForwardHeaders)
         {
            app = app.UseForwardedHeaders();
         }

         if (stackageOptions.SupportHttps)
         {
            app = app
               .UseHttpsRedirection()
               .UseHsts();
         }

         return app
            .UseMiddleware<LivenessMiddleware>()
            .UseMetricServer(prometheusOptions.MetricsEndpoint)
            .UseMiddleware<ReadinessMiddleware>()
            .UseMiddleware<StartupTasksMiddleware>()
            .UseMiddleware<MetricsAndExceptionHandlingMiddleware>()
            .UseMiddleware<RateLimitingMiddleware>()
            .UseMiddleware<HealthMiddleware>();
      }
   }
}
