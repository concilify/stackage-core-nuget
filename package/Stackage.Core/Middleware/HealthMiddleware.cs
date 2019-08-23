using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Stackage.Core.Abstractions;
using Stackage.Core.Extensions;

namespace Stackage.Core.Middleware
{
   // ReSharper disable once ClassNeverInstantiated.Global
   public class HealthMiddleware
   {
      private readonly JsonSerializerSettings _jsonSerialiserSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};

      public HealthMiddleware(RequestDelegate _)
      {
      }

      public async Task Invoke(
         HttpContext context,
         HealthCheckService healthCheckService,
         IServiceInfo serviceInfo)
      {
         var healthReport = await healthCheckService.CheckHealthAsync((_) => true, context.RequestAborted);

         await context.Response.WriteJsonAsync(
            GetStatusCode(healthReport.Status),
            GetContent(healthReport, serviceInfo),
            _jsonSerialiserSettings);
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

      private static object GetDependencies(HealthReport healthReport)
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

      private static object GetException(HealthReportEntry entry)
      {
         return entry.Exception?.GetType().FullName;
      }

      private static object GetData(HealthReportEntry entry)
      {
         if (entry.Data == null || entry.Data.Count == 0)
         {
            return null;
         }

         return entry.Data;
      }
   }
}
