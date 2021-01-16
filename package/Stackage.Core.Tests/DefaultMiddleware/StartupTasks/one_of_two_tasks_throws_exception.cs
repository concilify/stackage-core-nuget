using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Abstractions.StartupTasks;

namespace Stackage.Core.Tests.DefaultMiddleware.StartupTasks
{
   public class one_of_two_tasks_throws_exception : middleware_scenario
   {
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _response = await TestService.GetAsync("/get");
         _content = await _response.Content.ReadAsStringAsync();
      }

      protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
      {
         base.ConfigureServices(services, configuration);

         services.AddTransient<IStartupTask>(_ => new StubStartupTask());
         services.AddTransient<IStartupTask>(_ => new StubStartupTask {ThrowException = new Exception("Task failed")});
      }

      protected override void Configure(IApplicationBuilder app)
      {
         base.Configure(app);

         app.UseMiddleware<StubResponseMiddleware>(new StubResponseOptions(HttpStatusCode.OK, "content post startup"));
      }

      [Test]
      public void should_return_status_code_503()
      {
         _response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
      }

      [Test]
      public void should_return_content()
      {
         _content.ShouldBe("Service Unavailable");
      }

      [Test]
      public void should_log_four_errors()
      {
         StartupTasksExecutorLogger.Entries.Count.ShouldBe(4);
      }

      [Test]
      public void should_log_error_message()
      {
         var errorEntry = StartupTasksExecutorLogger.Entries.Single(c => c.LogLevel == LogLevel.Error);

         errorEntry.Exception.InnerException?.Message.ShouldBe("Task failed");
      }

      [Test]
      public void should_write_one_metric()
      {
         Assert.That(MetricSink.Metrics.Count, Is.EqualTo(1));
      }

      [Test]
      public void should_write_not_ready_metric()
      {
         var metric = (Counter) MetricSink.Metrics.Last();

         Assert.That(metric.Name, Is.EqualTo("not_ready"));
         Assert.That(metric.Dimensions["method"], Is.EqualTo("GET"));
      }
   }
}
