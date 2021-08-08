using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Stackage.Core.Tests
{
   public class StubHealthCheck : IHealthCheck
   {
      public HealthCheckResult CheckHealthResponse { get; set; }

      public HealthStatus LastFailureStatus { get; private set; }

      public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
      {
         LastFailureStatus = context.Registration.FailureStatus;

         await Task.Yield();

         return CheckHealthResponse;
      }
   }
}
