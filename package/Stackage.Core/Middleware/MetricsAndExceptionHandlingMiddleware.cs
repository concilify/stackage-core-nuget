using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Stackage.Core.Abstractions;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Abstractions.Polly;
using Stackage.Core.Extensions;

namespace Stackage.Core.Middleware
{
   // ReSharper disable once ClassNeverInstantiated.Global
   public class MetricsAndExceptionHandlingMiddleware
   {
      private readonly RequestDelegate _next;
      private readonly IPolicyFactory _policyFactory;
      private readonly IMetricSink _metricSink;
      private readonly IGuidGenerator _guidGenerator;
      private readonly ILogger<MetricsAndExceptionHandlingMiddleware> _logger;

      public MetricsAndExceptionHandlingMiddleware(
         RequestDelegate next,
         IPolicyFactory policyFactory,
         IMetricSink metricSink,
         IGuidGenerator guidGenerator,
         ILogger<MetricsAndExceptionHandlingMiddleware> logger)
      {
         _next = next ?? throw new ArgumentNullException(nameof(next));
         _policyFactory = policyFactory ?? throw new ArgumentNullException(nameof(policyFactory));
         _metricSink = metricSink ?? throw new ArgumentNullException(nameof(metricSink));
         _guidGenerator = guidGenerator ?? throw new ArgumentNullException(nameof(guidGenerator));
         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      }

      public async Task Invoke(HttpContext httpContext)
      {
         Task OnSuccessAsync(Context policyContext)
         {
            policyContext.Add("statusCode", httpContext.Response.StatusCode);
            return Task.CompletedTask;
         }

         var metricsPolicy = _policyFactory.CreateAsyncMetricsPolicy("http_request", _metricSink, onSuccessAsync: OnSuccessAsync);

         var dimensions = new Dictionary<string, object>
         {
            {"method", httpContext.Request.Method},
            {"path", $"{httpContext.Request.PathBase}{httpContext.Request.Path}"}
         };

         await metricsPolicy.ExecuteAsync(policyContext => Invoke(httpContext, policyContext), dimensions);
      }

      private async Task Invoke(HttpContext httpContext, Context policyContext)
      {
         try
         {
            await _next(httpContext);
         }
         catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
         {
            await httpContext.Response.WriteJsonAsync((HttpStatusCode) 499, new {message = "Client Closed Request"});
         }
         catch (AuthenticationException e)
         {
            var token = _guidGenerator.GenerateToken();

            _logger.LogWarning(e, "An authentication exception has occurred (token={token})", token);

            await httpContext.Response.WriteJsonAsync(HttpStatusCode.Unauthorized, new {message = "Unauthorized", token});
         }
         catch (Exception e)
         {
            var token = _guidGenerator.GenerateToken();

            _logger.LogError(e, "An unexpected exception has occurred (token={token})", token);

            policyContext.Add("exception", e.GetType().Name);

            await httpContext.Response.WriteJsonAsync(HttpStatusCode.InternalServerError, new {message = "Internal Server Error", token});
         }
      }
   }
}
