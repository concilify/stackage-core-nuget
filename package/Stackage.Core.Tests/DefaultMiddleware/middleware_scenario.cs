using FakeItEasy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Stackage.Core.Abstractions;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Extensions;
using Stackage.Core.Middleware;
using Stackage.Core.StartupTasks;

namespace Stackage.Core.Tests.DefaultMiddleware
{
   public abstract class middleware_scenario
   {
      protected IGuidGenerator GuidGenerator { get; private set; }
      protected IServiceInfo ServiceInfo { get; private set; }
      protected StubMetricSink MetricSink { get; private set; }
      protected StubLogger<ExceptionHandlingMiddleware> Logger { get; private set; }
      protected StubLogger<StartupTasksExecutor> StartupTasksLogger { get; private set; }
      protected TestService TestService { get; private set; }

      [OneTimeSetUp]
      public void setup_scenario_base()
      {
         ConfigureDependencies();
         TestService = new TestService(ConfigureWebHostBuilder, ConfigureConfiguration, ConfigureServices, Configure);
      }

      protected virtual void ConfigureDependencies()
      {
         GuidGenerator = A.Fake<IGuidGenerator>();
         ServiceInfo = A.Fake<IServiceInfo>();
         MetricSink = new StubMetricSink();
         Logger = new StubLogger<ExceptionHandlingMiddleware>();
         StartupTasksLogger = new StubLogger<StartupTasksExecutor>();

         A.CallTo(() => GuidGenerator.Generate()).Returns("not-a-guid");
         A.CallTo(() => ServiceInfo.Service).Returns("service");
         A.CallTo(() => ServiceInfo.Version).Returns("version");
         A.CallTo(() => ServiceInfo.Host).Returns("host");
      }

      protected virtual void ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder)
      {
      }

      protected virtual void ConfigureConfiguration(IConfigurationBuilder configurationBuilder)
      {
      }

      protected virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
      {
         services.AddDefaultServices(configuration);

         services.AddSingleton(GuidGenerator);
         services.AddSingleton(ServiceInfo);
         services.AddSingleton<IMetricSink>(MetricSink);
         services.AddSingleton<ILogger<ExceptionHandlingMiddleware>>(Logger);
         services.AddSingleton<ILogger<StartupTasksExecutor>>(StartupTasksLogger);
      }

      protected virtual void Configure(IApplicationBuilder app)
      {
         var hostEnvironment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

         app.UseDefaultMiddleware(hostEnvironment);
      }
   }
}
