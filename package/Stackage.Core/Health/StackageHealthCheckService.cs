using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Stackage.Core.Polly.Metrics;

namespace Stackage.Core.Health
{
   public class StackageHealthCheckService : HealthCheckService
   {
      private readonly IServiceScopeFactory _scopeFactory;
      private readonly ITimerFactory _timerFactory;
      private readonly IOptions<HealthCheckServiceOptions> _options;

      public StackageHealthCheckService(
         IServiceScopeFactory scopeFactory,
         ITimerFactory timerFactory,
         IOptions<HealthCheckServiceOptions> options)
      {
         _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
         _timerFactory = timerFactory;
         _options = options ?? throw new ArgumentNullException(nameof(options));
      }

      public override async Task<HealthReport> CheckHealthAsync(
         Func<HealthCheckRegistration, bool>? predicate,
         CancellationToken cancellationToken = default)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            var registrations = _options.Value.Registrations;
            var timer = _timerFactory.CreateAndStart();

            var heathChecks = registrations
               .Select(registration => new
               {
                  registration.Name,
                  HealthReportEntry = CheckHealthAsync(registration.Factory, scope.ServiceProvider, registration)
               })
               .ToArray();

            await Task.WhenAll(heathChecks.Select(c => c.HealthReportEntry));

            var healthReport = new HealthReport(heathChecks.ToDictionary(c => c.Name, c => c.HealthReportEntry.Result), TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds));
            return healthReport;
         }
      }

      private async Task<HealthReportEntry> CheckHealthAsync(
         Func<IServiceProvider, IHealthCheck> healthCheckFactory,
         IServiceProvider serviceProvider,
         HealthCheckRegistration registration)
      {
         var timer = _timerFactory.CreateAndStart();

         var healthCheck = healthCheckFactory(serviceProvider);
         var context = new HealthCheckContext {Registration = registration};

         var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

         timer.Stop();

         return new HealthReportEntry(
            result.Status,
            string.IsNullOrWhiteSpace(result.Description) ? null : result.Description,
            TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds),
            result.Exception,
            result.Data);
      }
   }
}
