using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Stackage.Core.Tests
{
   public class StubHealthCheck : IHealthCheck
   {
      public TimeSpan? Latency { get; set; }

      public HealthCheckResult CheckHealthResponse { get; set; }

      public HealthStatus LastFailureStatus { get; private set; }

      public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
      {
         LastFailureStatus = context.Registration.FailureStatus;

         if (Latency != null)
         {
            await Task.Delay(Latency.Value, cancellationToken);
         }
         else
         {
            await Task.Delay(1, cancellationToken);
         }

         return CheckHealthResponse;
      }
   }
}
