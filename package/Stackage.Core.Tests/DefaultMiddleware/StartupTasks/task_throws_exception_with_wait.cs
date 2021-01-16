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
   public class task_throws_exception_with_wait : middleware_scenario
   {
      private Exception _exceptionToThrow;
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         using (var fakeServer = TestService.CreateServer())
         {
            await Task.Delay(1000);

            _response = await TestService.GetAsync(fakeServer, "/get");
            _content = await _response.Content.ReadAsStringAsync();
         }
      }

      protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
      {
         base.ConfigureServices(services, configuration);

         _exceptionToThrow = new Exception();

         services.AddTransient<IStartupTask>(_ => new StubStartupTask {ThrowException = _exceptionToThrow});
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
      public void should_log_two_messages()
      {
         StartupTasksExecutorLogger.Entries.Count.ShouldBe(2);
      }

      [Test]
      public void should_log_startup_message()
      {
         StartupTasksExecutorLogger.Entries[0].LogLevel.ShouldBe(LogLevel.Information);
         StartupTasksExecutorLogger.Entries[0].Message.ShouldBe("Startup task StubStartupTask executing...");
      }

      [Test]
      public void should_log_failed_message()
      {
         StartupTasksExecutorLogger.Entries[1].LogLevel.ShouldBe(LogLevel.Error);
         StartupTasksExecutorLogger.Entries[1].Message.ShouldBe("Startup task StubStartupTask failed");
         StartupTasksExecutorLogger.Entries[1].Exception.ShouldBeSameAs(_exceptionToThrow);
      }

      [Test]
      public void should_log_warning_message_in_middleware()
      {
         StartupTasksMiddlewareLogger.Entries.Count.ShouldBe(1);
      }

      [Test]
      public void should_log_message_containing_path()
      {
         StartupTasksMiddlewareLogger.Entries[0].LogLevel.ShouldBe(LogLevel.Warning);
         StartupTasksMiddlewareLogger.Entries[0].Message.ShouldBe("Unable to fulfill request /get");
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
