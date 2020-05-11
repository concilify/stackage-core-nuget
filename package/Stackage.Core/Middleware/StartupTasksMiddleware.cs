using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Stackage.Core.Abstractions.StartupTasks;

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
         IStartupTasksExecutor startupTasksExecutor)
      {
         if (startupTasksExecutor.AllCompleteAndSuccessful)
         {
            await _next(context);
         }
         else
         {
            context.Response.StatusCode = 503;
            context.Response.Headers["Retry-After"] = "30";

            await context.Response.WriteAsync("Service Unavailable");
         }
      }
   }
}
