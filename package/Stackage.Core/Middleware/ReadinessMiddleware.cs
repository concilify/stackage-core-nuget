using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Stackage.Core.Abstractions.StartupTasks;
using Stackage.Core.Extensions;
using Stackage.Core.Middleware.Options;

namespace Stackage.Core.Middleware
{
   // ReSharper disable once ClassNeverInstantiated.Global
   public class ReadinessMiddleware
   {
      private readonly RequestDelegate _next;
      private readonly string _readinessEndpoint;

      public ReadinessMiddleware(RequestDelegate next, IOptions<HealthOptions> options)
      {
         if (options == null) throw new ArgumentNullException(nameof(options));

         _next = next ?? throw new ArgumentNullException(nameof(next));

         _readinessEndpoint = options.Value.ReadinessEndpoint;
      }

      public async Task Invoke(
         HttpContext context,
         IStartupTasksExecutor startupTasksExecutor,
         HealthCheckService healthCheckService)
      {
         if (!context.Request.Path.Equals(_readinessEndpoint))
         {
            await _next(context);

            return;
         }

         if (startupTasksExecutor.AllCompleteAndSuccessful)
         {
            var healthReport = await healthCheckService.CheckHealthAsync((_) => true, context.RequestAborted);

            context.Response.AddNoCacheHeaders();

            await context.Response.WriteTextAsync(GetStatusCode(healthReport.Status), healthReport.Status.ToString());
         }
         else
         {
            await context.Response.WriteServiceUnavailableAsync();
         }
      }

      private static HttpStatusCode GetStatusCode(HealthStatus healthStatus)
      {
         if (healthStatus == HealthStatus.Healthy || healthStatus == HealthStatus.Degraded)
         {
            return HttpStatusCode.OK;
         }

         return HttpStatusCode.ServiceUnavailable;
      }
   }
}
