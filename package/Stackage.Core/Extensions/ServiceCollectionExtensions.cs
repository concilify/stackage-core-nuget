using System;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Core.Abstractions;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Abstractions.Polly;
using Stackage.Core.Metrics;
using Stackage.Core.Polly;

namespace Stackage.Core.Extensions
{
   public static class ServiceCollectionExtensions
   {
      public static IServiceCollection AddDefaultServices(this IServiceCollection services)
      {
         if (services == null) throw new ArgumentNullException(nameof(services));

         services.AddSingleton<IGuidGenerator, GuidGenerator>();
         services.AddSingleton<IPolicyFactory, PolicyFactory>();
         services.AddSingleton<IMetricSink, LoggingMetricSink>();

         return services;
      }
   }
}
