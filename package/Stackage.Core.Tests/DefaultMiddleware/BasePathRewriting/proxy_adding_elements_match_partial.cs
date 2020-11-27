using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shouldly;

namespace Stackage.Core.Tests.DefaultMiddleware.BasePathRewriting
{
   public class proxy_adding_elements_match_partial : middleware_scenario
   {
      private HttpResponseMessage _response1;
      private HttpResponseMessage _response2;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         // Proxy receives /foo/get and forwards /a/foo/get
         _response1 = await TestService.GetAsync("http://localhost:5000/a/foo/get");

         // Proxy receives /bar/get and forwards /b/bar/get
         _response2 = await TestService.GetAsync("http://localhost:5000/b/bar/get");
      }

      protected override void ConfigureConfiguration(IConfigurationBuilder configurationBuilder)
      {
         base.ConfigureConfiguration(configurationBuilder);

         configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
         {
            {"STACKAGE:BASEPATHREWRITING:RULES:0:MATCH", "/a"},
            {"STACKAGE:BASEPATHREWRITING:RULES:0:ADDED", "/a"},
            {"STACKAGE:BASEPATHREWRITING:RULES:1:MATCH", "/b"},
            {"STACKAGE:BASEPATHREWRITING:RULES:1:ADDED", "/b"}
         });
      }

      protected override void Configure(IApplicationBuilder app)
      {
         base.Configure(app);

         app.UseMiddleware<StubResponseMiddleware>(new StubResponseOptions {Handler = StubHandler});
      }

      protected async Task StubHandler(HttpContext context)
      {
         var serviceInfo = new ServiceInfo(new HttpContextAccessor {HttpContext = context});

         await context.Response.WriteAsync(serviceInfo.BaseAddress, Encoding.UTF8);
      }

      [Test]
      public async Task should_compose_base_address_for_request_1()
      {
         var content = await _response1.Content.ReadAsStringAsync();

         content.ShouldBe("http://localhost:5000");
      }

      [Test]
      public async Task should_compose_base_address_for_request_2()
      {
         var content = await _response2.Content.ReadAsStringAsync();

         content.ShouldBe("http://localhost:5000");
      }

      [Test]
      public void should_metrics_for_request_1()
      {
         var metrics = MetricSink.Metrics.ToArray();

         Assert.That(metrics[0].Dimensions["path"], Is.EqualTo("/foo/get"));
         Assert.That(metrics[1].Dimensions["path"], Is.EqualTo("/foo/get"));
      }

      [Test]
      public void should_metrics_for_request_2()
      {
         var metrics = MetricSink.Metrics.ToArray();

         Assert.That(metrics[2].Dimensions["path"], Is.EqualTo("/bar/get"));
         Assert.That(metrics[3].Dimensions["path"], Is.EqualTo("/bar/get"));
      }
   }
}
