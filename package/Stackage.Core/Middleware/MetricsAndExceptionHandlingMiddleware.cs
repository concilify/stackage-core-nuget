using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Stackage.Core.Abstractions;
using Stackage.Core.Abstractions.Polly;
using Stackage.Core.Extensions;

namespace Stackage.Core.Middleware
{
   // ReSharper disable once ClassNeverInstantiated.Global
   public class MetricsAndExceptionHandlingMiddleware
   {
      private readonly RequestDelegate _next;

      public MetricsAndExceptionHandlingMiddleware(RequestDelegate next)
      {
         _next = next ?? throw new ArgumentNullException(nameof(next));
      }

      public async Task Invoke(
         HttpContext httpContext,
         IPolicyFactory policyFactory,
         ITokenGenerator tokenGenerator,
         IJsonSerialiser jsonSerialiser,
         ILogger<MetricsAndExceptionHandlingMiddleware> logger)
      {
         Task OnSuccessAsync(Context policyContext)
         {
            policyContext.Add("statusCode", httpContext.Response.StatusCode);
            return Task.CompletedTask;
         }

         var metricsPolicy = policyFactory.CreateAsyncMetricsPolicy("http_request", onSuccessAsync: OnSuccessAsync);

         var dimensions = new Dictionary<string, object>
         {
            {"method", httpContext.Request.Method},
            {"path", $"{httpContext.Request.PathBase}{httpContext.Request.Path}"}
         };

         await metricsPolicy.ExecuteAsync(policyContext => InvokeAsync(httpContext, policyContext, tokenGenerator, jsonSerialiser, logger), dimensions);
      }

      private async Task InvokeAsync(
         HttpContext httpContext,
         Context policyContext,
         ITokenGenerator tokenGenerator,
         IJsonSerialiser jsonSerialiser,
         ILogger<MetricsAndExceptionHandlingMiddleware> logger)
      {
         try
         {
            await _next(httpContext);
         }
         catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
         {
            await httpContext.Response.WriteJsonAsync((HttpStatusCode) 499, new {message = "Client Closed Request"}, jsonSerialiser);
         }
         catch (AuthenticationException e)
         {
            var token = tokenGenerator.Generate();

            logger.LogWarning(e, "An authentication exception has occurred (token={token})", token);

            await httpContext.Response.WriteJsonAsync(HttpStatusCode.Unauthorized, new {message = "Unauthorized", token}, jsonSerialiser);
         }
         catch (Exception e)
         {
            var token = tokenGenerator.Generate();

            logger.LogError(e, "An unexpected exception has occurred (token={token})", token);

            policyContext.Add("exception", e.GetType().Name);

            await httpContext.Response.WriteJsonAsync(HttpStatusCode.InternalServerError, new {message = "Internal Server Error", token}, jsonSerialiser);
         }
      }
   }
}
