using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Stackage.Core.Abstractions;
using Stackage.Core.Extensions;
using Stackage.Core.Options;

namespace Stackage.Core.Middleware
{
   // ReSharper disable once ClassNeverInstantiated.Global
   public class HealthMiddleware
   {
      private readonly RequestDelegate _next;
      private readonly string _healthEndpoint;

      public HealthMiddleware(RequestDelegate next, IOptions<HealthOptions> options)
      {
         if (options == null) throw new ArgumentNullException(nameof(options));

         _next = next ?? throw new ArgumentNullException(nameof(next));

         _healthEndpoint = options.Value.Endpoint;
      }

      public async Task Invoke(
         HttpContext context,
         HealthCheckService healthCheckService,
         IServiceInfo serviceInfo,
         IJsonSerialiser jsonSerialiser)
      {
         if (!context.Request.Path.Equals(_healthEndpoint))
         {
            await _next(context);

            return;
         }

         var healthReport = await healthCheckService.CheckHealthAsync((_) => true, context.RequestAborted);

         context.Response.AddNoCacheHeaders();

         await context.Response.WriteJsonAsync(
            GetStatusCode(healthReport.Status),
            GetContent(healthReport, serviceInfo),
            jsonSerialiser);
      }

      private static HttpStatusCode GetStatusCode(HealthStatus healthStatus)
      {
         if (healthStatus == HealthStatus.Healthy || healthStatus == HealthStatus.Degraded)
         {
            return HttpStatusCode.OK;
         }

         return HttpStatusCode.ServiceUnavailable;
      }

      private static object GetContent(HealthReport healthReport, IServiceInfo serviceInfo)
      {
         return new
         {
            service = serviceInfo.Service,
            version = serviceInfo.Version,
            host = serviceInfo.Host,
            status = healthReport.Status.ToString(),
            durationMs = (long) healthReport.TotalDuration.TotalMilliseconds,
            dependencies = GetDependencies(healthReport)
         };
      }

      private static object? GetDependencies(HealthReport healthReport)
      {
         if (healthReport.Entries == null || healthReport.Entries.Count == 0)
         {
            return null;
         }

         return healthReport.Entries.Select(GetDependency);
      }

      private static object GetDependency(KeyValuePair<string, HealthReportEntry> healthReportEntry)
      {
         var entry = healthReportEntry.Value;

         return new
         {
            name = healthReportEntry.Key,
            description = entry.Description,
            status = entry.Status.ToString(),
            durationMs = (long) entry.Duration.TotalMilliseconds,
            exception = GetException(entry),
            data = GetData(entry)
         };
      }

      private static object? GetException(HealthReportEntry entry)
      {
         return entry.Exception?.GetType().FullName;
      }

      private static object? GetData(HealthReportEntry entry)
      {
         if (entry.Data == null || entry.Data.Count == 0)
         {
            return null;
         }

         return entry.Data;
      }
   }
}
