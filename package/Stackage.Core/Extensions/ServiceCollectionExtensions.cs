using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Stackage.Core.Abstractions;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Abstractions.Polly;
using Stackage.Core.Health;
using Stackage.Core.Metrics;
using Stackage.Core.Polly;

namespace Stackage.Core.Extensions
{
   public static class ServiceCollectionExtensions
   {
      public static IServiceCollection AddDefaultServices(this IServiceCollection services)
      {
         if (services == null) throw new ArgumentNullException(nameof(services));

         services.AddTransient<IGuidGenerator, GuidGenerator>();
         services.AddTransient<IServiceInfo, ServiceInfo>();
         services.AddTransient<HealthCheckService, StackageHealthCheckService>();
         services.AddTransient<IPolicyFactory, PolicyFactory>();
         services.AddTransient<IMetricSink, LoggingMetricSink>();
         services.AddHsts(options =>
         {
            options.MaxAge = TimeSpan.FromDays(365);
            options.IncludeSubDomains = true;
         });

         return services;
      }

      public static IServiceCollection AddHealthCheck(this IServiceCollection services, string name, IHealthCheck healthCheck)
      {
         var registration = new HealthCheckRegistration(name, healthCheck, null, null);

         services.Configure<HealthCheckServiceOptions>(options => options.Registrations.Add(registration));

         return services;
      }

      public static IServiceCollection AddHealthCheck(this IServiceCollection services, string name, Func<IServiceProvider, IHealthCheck> healthCheckFactory)
      {
         services.AddOptions();
         services.AddSingleton<IConfigureOptions<HealthCheckServiceOptions>>(sp =>
         {
            var healthCheck = healthCheckFactory(sp);
            var registration = new HealthCheckRegistration(name, healthCheck, null, null);

            return new ConfigureNamedOptions<HealthCheckServiceOptions>(Options.DefaultName, options => options.Registrations.Add(registration));
         });

         return services;
      }

      public static IServiceCollection AddHealthCheck<THealthCheck>(this IServiceCollection services, string name)
         where THealthCheck : IHealthCheck
      {
         services.AddOptions();
         services.AddSingleton<IConfigureOptions<HealthCheckServiceOptions>>(sp =>
         {
            var healthCheck = (IHealthCheck)ActivatorUtilities.CreateInstance(sp, typeof(THealthCheck));
            var registration = new HealthCheckRegistration(name, healthCheck, null, null);

            return new ConfigureNamedOptions<HealthCheckServiceOptions>(Options.DefaultName, options => options.Registrations.Add(registration));
         });

         return services;
      }
   }
}
