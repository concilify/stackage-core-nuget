using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.StartupTasks;

namespace Stackage.Core.Tests.DefaultMiddleware.Liveness
{
   public class slow_startup_task : middleware_scenario
   {
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _response = await TestService.GetAsync("/health/liveness");
         _content = await _response.Content.ReadAsStringAsync();
      }

      protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
      {
         base.ConfigureServices(services, configuration);

         services.AddTransient<IStartupTask>(_ => new StubStartupTask {Latency = TimeSpan.FromSeconds(3)});
      }

      [Test]
      public void should_return_status_code_200()
      {
         _response.StatusCode.ShouldBe(HttpStatusCode.OK);
      }

      [Test]
      public void should_return_content_healthy()
      {
         _content.ShouldBe("Healthy");
      }

      [Test]
      public void should_return_content_type_text_plain()
      {
         _response.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
      }
   }
}
