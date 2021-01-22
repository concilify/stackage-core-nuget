using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Stackage.Core.Extensions;
using Stackage.Core.Options;

namespace Stackage.Core.Middleware
{
   // ReSharper disable once ClassNeverInstantiated.Global
   public class LivenessMiddleware
   {
      private readonly RequestDelegate _next;
      private readonly string _livenessEndpoint;

      public LivenessMiddleware(RequestDelegate next, IOptions<HealthOptions> options)
      {
         if (options == null) throw new ArgumentNullException(nameof(options));

         _next = next ?? throw new ArgumentNullException(nameof(next));

         _livenessEndpoint = options.Value.LivenessEndpoint;
      }

      public async Task Invoke(HttpContext context)
      {
         if (!context.Request.Path.Equals(_livenessEndpoint))
         {
            await _next(context);

            return;
         }

         context.Response.AddNoCacheHeaders();

         await context.Response.WriteTextAsync(HttpStatusCode.OK, HealthStatus.Healthy.ToString());
      }
   }
}
