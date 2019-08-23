using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Stackage.Core.Tests
{
   // TODO: Is this needed?
   public class StubHealthCheckService : HealthCheckService
   {
      public TimeSpan? Latency { get; set; }

      public HealthReport CheckHealthResponse { get; set; }

      public override async Task<HealthReport> CheckHealthAsync(Func<HealthCheckRegistration, bool> predicate,
         CancellationToken cancellationToken = new CancellationToken())
      {
         if (Latency != null)
         {
            await Task.Delay(Latency.Value, cancellationToken);
         }

         return CheckHealthResponse;
      }
   }
}
