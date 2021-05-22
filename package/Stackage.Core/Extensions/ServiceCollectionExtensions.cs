using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Stackage.Core.Abstractions;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Abstractions.Polly;
using Stackage.Core.Abstractions.StartupTasks;
using Stackage.Core.Health;
using Stackage.Core.MetricSinks;
using Stackage.Core.Options;
using Stackage.Core.Polly;
using Stackage.Core.StartupTasks;

namespace Stackage.Core.Extensions
{
   public static class ServiceCollectionExtensions
   {
      public static IServiceCollection AddDefaultServices(this IServiceCollection services, IConfiguration configuration)
      {
         if (services == null) throw new ArgumentNullException(nameof(services));

         services.AddHttpContextAccessor();

         services.AddHostedService<StartupTasksBackgroundService>();

         services.AddSingleton<IStartupTasksExecutor, StartupTasksExecutor>();
         services.AddSingleton<PrometheusMetricSink>();
         services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<PrometheusMetricSink>());
         services.AddSingleton<IMetricSink>(sp => sp.GetRequiredService<PrometheusMetricSink>());

         services.AddTransient<IGuidGenerator, GuidGenerator>();
         services.AddTransient<ITokenGenerator, TokenGenerator>();
         services.AddTransient<IServiceInfo, ServiceInfo>();
         services.AddTransient<HealthCheckService, StackageHealthCheckService>();
         services.AddTransient<IPolicyFactory, PolicyFactory>();

         var stackageConfiguration = configuration.GetSection("stackage");

         services.Configure<StackageOptions>(stackageConfiguration);
         services.Configure<HealthOptions>(stackageConfiguration.GetSection("health"));
         services.Configure<RateLimitingOptions>(stackageConfiguration.GetSection("ratelimiting"));
         services.Configure<PrometheusOptions>(stackageConfiguration.GetSection("prometheus"));

         services.Configure<ForwardedHeadersOptions>(options =>
         {
            options.ForwardedHeaders = ForwardedHeaders.All;

            // Remove default networks and proxies - if not empty would need to supply IP address of Docker network
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
         });

         services.Configure<HstsOptions>(options =>
         {
            options.MaxAge = TimeSpan.FromDays(365);
            options.IncludeSubDomains = true;
         });

         return services;
      }

      public static IServiceCollection AddHealthCheck(
         this IServiceCollection services,
         string name,
         IHealthCheck healthCheck,
         HealthStatus? failureStatus = null)
      {
         var registration = new HealthCheckRegistration(name, healthCheck, failureStatus, null);

         services.Configure<HealthCheckServiceOptions>(options => { options.Registrations.Add(registration); });

         return services;
      }

      public static IServiceCollection AddHealthCheck(
         this IServiceCollection services,
         string name,
         Func<IServiceProvider, IHealthCheck> healthCheckFactory,
         HealthStatus? failureStatus = null)
      {
         var registration = new HealthCheckRegistration(name, healthCheckFactory, failureStatus, null, null);

         services.Configure<HealthCheckServiceOptions>(options => { options.Registrations.Add(registration); });

         return services;
      }

      public static IServiceCollection AddHealthCheck<THealthCheck>(
         this IServiceCollection services,
         string name,
         HealthStatus? failureStatus = null)
         where THealthCheck : IHealthCheck
      {
         var registration = new HealthCheckRegistration(name, sp => ActivatorUtilities.CreateInstance<THealthCheck>(sp), failureStatus, null, null);

         services.Configure<HealthCheckServiceOptions>(options => { options.Registrations.Add(registration); });

         return services;
      }

      public static IServiceCollection AddGenericImplementations(
         this IServiceCollection services,
         Type genericServiceType,
         ITypeEnumerator discoverFromTypes,
         ServiceLifetime lifetime)
      {
         var implementations = discoverFromTypes.GetGenericTypes(genericServiceType);

         foreach (var implementation in implementations)
         {
            foreach (var service in implementation.Services)
            {
               services.Add(new ServiceDescriptor(service.ServiceType, implementation.ImplementationType, lifetime));
            }
         }

         return services;
      }
   }
}
