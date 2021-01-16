using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Abstractions.StartupTasks;
using Stackage.Core.Extensions;

namespace Stackage.Core.Middleware
{
   public class StartupTasksMiddleware
   {
      private readonly RequestDelegate _next;

      public StartupTasksMiddleware(RequestDelegate next)
      {
         _next = next ?? throw new ArgumentNullException(nameof(next));
      }

      public async Task Invoke(
         HttpContext context,
         IStartupTasksExecutor startupTasksExecutor,
         IMetricSink metricSink,
         ILogger<StartupTasksMiddleware> logger)
      {
         if (startupTasksExecutor.AllCompleteAndSuccessful)
         {
            await _next(context);

            return;
         }

         await metricSink.PushAsync(new Counter
         {
            Name = "not_ready",
            Dimensions = new Dictionary<string, object> {{"method", context.Request.Method}}
         });

         logger.LogWarning("Unable to fulfill request {@path}", context.Request.Path);

         await context.Response.WriteServiceUnavailableAsync();
      }
   }
}
