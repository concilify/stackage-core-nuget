using FakeItEasy;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Stackage.Core.Abstractions;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Extensions;
using Stackage.Core.Middleware;

namespace Stackage.Core.Tests.DefaultMiddleware
{
   public abstract class middleware_scenario
   {
      protected IGuidGenerator GuidGenerator { get; private set; }
      protected StubMetricSink MetricSink { get; private set; }
      protected StubLogger<ExceptionHandlingMiddleware> Logger { get; private set; }
      protected TestService TestService { get; private set; }

      [OneTimeSetUp]
      public void setup_scenario_base()
      {
         GuidGenerator = A.Fake<IGuidGenerator>();
         MetricSink = new StubMetricSink();
         Logger = new StubLogger<ExceptionHandlingMiddleware>();

         TestService = new TestService(configureServices: ConfigureServices, configure: Configure);
      }

      protected virtual void ConfigureServices(IServiceCollection services)
      {
         services.AddDefaultServices();
         services.AddSingleton(GuidGenerator);
         services.AddSingleton<IMetricSink>(MetricSink);
         services.AddSingleton<ILogger<ExceptionHandlingMiddleware>>(Logger);
      }

      protected virtual void Configure(IApplicationBuilder app)
      {
         app.UseDefaultMiddleware();
      }
   }
}
