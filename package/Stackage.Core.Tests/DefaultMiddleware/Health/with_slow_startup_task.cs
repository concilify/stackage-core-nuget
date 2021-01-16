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

namespace Stackage.Core.Tests.DefaultMiddleware.Health
{
   public class with_slow_startup_task : middleware_scenario
   {
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         using (var fakeServer = TestService.CreateServer())
         {
            await Task.Delay(500);

            _response = await TestService.GetAsync(fakeServer, "/health");
            _content = await _response.Content.ReadAsStringAsync();
         }
      }

      protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
      {
         base.ConfigureServices(services, configuration);

         services.AddTransient<IStartupTask>(_ => new StubStartupTask {Latency = TimeSpan.FromSeconds(3)});
      }

      protected override void Configure(IApplicationBuilder app)
      {
         base.Configure(app);

         app.UseMiddleware<StubResponseMiddleware>(new StubResponseOptions(HttpStatusCode.OK, "content post wait"));
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
      public void should_return_content_type_text_plain()
      {
         _response.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
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
         StartupTasksMiddlewareLogger.Entries[0].Message.ShouldBe("Unable to fulfill request /health");
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
