using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace Stackage.Core.Tests.DefaultMiddleware.Security.BehindProxyTrue
{
   public class https_request_dev : middleware_scenario
   {
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _response = await TestService.GetAsync("https://localhost:5001/get");
         _content = await _response.Content.ReadAsStringAsync();
      }

      protected override void ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder)
      {
         base.ConfigureWebHostBuilder(webHostBuilder);

         webHostBuilder.UseSetting("environment", "Development");
         webHostBuilder.UseSetting("https_port", "5001");
         webHostBuilder.UseSetting("urls", "http://localhost:5000;https://localhost:5001");
      }

      protected override void ConfigureConfiguration(IConfigurationBuilder configurationBuilder)
      {
         base.ConfigureConfiguration(configurationBuilder);

         configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
         {
            {"STACKAGE:RUNNINGBEHINDPROXY", "true"}
         });
      }

      protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
      {
         base.ConfigureServices(services, configuration);

         services.Configure<HstsOptions>(options => { options.ExcludedHosts.Remove("localhost"); });
      }

      protected override void Configure(IApplicationBuilder app)
      {
         base.Configure(app);

         app.UseMiddleware<StubResponseMiddleware>(new StubResponseOptions(HttpStatusCode.OK, "content"));
      }

      [Test]
      public void should_return_status_code_200()
      {
         _response.StatusCode.ShouldBe(HttpStatusCode.OK);
      }

      [Test]
      public void should_return_content()
      {
         _content.ShouldBe("content");
      }

      [Test]
      public void should_not_add_hsts_to_response_header()
      {
         _response.Headers.Contains("strict-transport-security").ShouldBe(false);
      }
   }
}
