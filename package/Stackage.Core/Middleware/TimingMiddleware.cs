using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Polly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Abstractions.Polly;

namespace Stackage.Core.Middleware
{
   // ReSharper disable once ClassNeverInstantiated.Global
   public class TimingMiddleware
   {
      private readonly RequestDelegate _next;
      private readonly IPolicyFactory _policyFactory;
      private readonly IMetricSink _metricSink;

      public TimingMiddleware(
         RequestDelegate next,
         IPolicyFactory policyFactory,
         IMetricSink metricSink)
      {
         _next = next ?? throw new ArgumentNullException(nameof(next));
         _policyFactory = policyFactory ?? throw new ArgumentNullException(nameof(policyFactory));
         _metricSink = metricSink ?? throw new ArgumentNullException(nameof(metricSink));
      }

      public async Task Invoke(HttpContext context)
      {
         Task OnSuccessAsync(Context policyContext)
         {
            policyContext.Add("statusCode", context.Response.StatusCode);
            return Task.CompletedTask;
         }

         var timingPolicy = _policyFactory.CreateAsyncTimingPolicy("http_request", _metricSink, onSuccessAsync: OnSuccessAsync);

         var dimensions = new Dictionary<string, object>
         {
            {"method", context.Request.Method},
            {"path", context.Request.Path.ToString()}
         };

         await timingPolicy.ExecuteAsync((_) => _next(context), dimensions);
      }
   }
}
