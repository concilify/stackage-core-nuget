using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Stackage.Core.Health
{
   public class StackageHealthCheckService : HealthCheckService
   {
      private readonly IServiceScopeFactory _scopeFactory;
      private readonly IOptions<HealthCheckServiceOptions> _options;

      public StackageHealthCheckService(
         IServiceScopeFactory scopeFactory,
         IOptions<HealthCheckServiceOptions> options)
      {
         _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
         _options = options ?? throw new ArgumentNullException(nameof(options));
      }

      public override async Task<HealthReport> CheckHealthAsync(
         Func<HealthCheckRegistration, bool> predicate,
         CancellationToken cancellationToken = new CancellationToken())
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            var registrations = _options.Value.Registrations;
            var context = new HealthCheckContext();
            var stopwatch = Stopwatch.StartNew();

            var heathChecks = registrations
               .Select(c => new {c.Name, HealthReportEntry = CheckHealthAsync(c.Factory, scope.ServiceProvider, context, stopwatch)})
               .ToArray();

            await Task.WhenAll(heathChecks.Select(c => c.HealthReportEntry));

            var healthReport = new HealthReport(heathChecks.ToDictionary(c => c.Name, c => c.HealthReportEntry.Result), stopwatch.Elapsed);
            return healthReport;
         }
      }

      private static async Task<HealthReportEntry> CheckHealthAsync(Func<IServiceProvider, IHealthCheck> healthCheckFactory, IServiceProvider serviceProvider,
         HealthCheckContext context, Stopwatch stopwatch)
      {
         var healthCheck = healthCheckFactory(serviceProvider);

         var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

         return new HealthReportEntry(
            result.Status,
            string.IsNullOrWhiteSpace(result.Description) ? null : result.Description,
            stopwatch.Elapsed,
            result.Exception,
            result.Data);
      }
   }
}
