using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Stackage.Core.Abstractions;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Abstractions.Polly;
using Stackage.Core.Health;
using Stackage.Core.Metrics;
using Stackage.Core.Middleware.Options;
using Stackage.Core.Polly;

namespace Stackage.Core.Extensions
{
   public static class ServiceCollectionExtensions
   {
      public static IServiceCollection AddDefaultServices(this IServiceCollection services, IConfiguration configuration)
      {
         if (services == null) throw new ArgumentNullException(nameof(services));

         services.AddHttpContextAccessor();
         services.AddTransient<IGuidGenerator, GuidGenerator>();
         services.AddTransient<IServiceInfo, ServiceInfo>();
         services.AddTransient<HealthCheckService, StackageHealthCheckService>();
         services.AddTransient<IPolicyFactory, PolicyFactory>();
         services.AddTransient<IMetricSink, LoggingMetricSink>();

         services.Configure<DefaultMiddlewareOptions>(configuration);
         services.Configure<PingOptions>(configuration.GetSection("ping"));
         services.Configure<HealthOptions>(configuration.GetSection("health"));
         services.Configure<RateLimitingOptions>(configuration.GetSection("ratelimiting"));
         services.Configure<BasePathRewritingOptions>(configuration.GetSection("basepathrewriting"));

         services.AddHsts(hstsOptions =>
         {
            hstsOptions.MaxAge = TimeSpan.FromDays(365);
            hstsOptions.IncludeSubDomains = true;
         });

         return services;
      }

      public static IServiceCollection AddHealthCheck(this IServiceCollection services, string name, IHealthCheck healthCheck,
         HealthStatus? failureStatus = null)
      {
         var registration = new HealthCheckRegistration(name, healthCheck, failureStatus, null);

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
            var healthCheck = (IHealthCheck) ActivatorUtilities.CreateInstance(sp, typeof(THealthCheck));
            var registration = new HealthCheckRegistration(name, healthCheck, null, null);

            return new ConfigureNamedOptions<HealthCheckServiceOptions>(Options.DefaultName, options => options.Registrations.Add(registration));
         });

         return services;
      }
   }
}
