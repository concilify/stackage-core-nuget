using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Tests.DefaultMiddleware.BasePathRewriting
{
   public class with_passthrough : middleware_scenario
   {
      private string _baseAddress;
      private HttpResponseMessage _response;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _response = await TestService.GetAsync("http://localhost:5000/a/b/c");
      }

      protected override void ConfigureConfiguration(IConfigurationBuilder configurationBuilder)
      {
         base.ConfigureConfiguration(configurationBuilder);

         configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
         {
            {"STACKAGE:BASEPATHREWRITING:RULES:0:MATCH", ""}
         });
      }

      protected override void Configure(IApplicationBuilder app)
      {
         base.Configure(app);

         app.UseMiddleware<StubResponseMiddleware>(new StubResponseOptions(HttpStatusCode.OK, "content") {Handler = StubHandler});
      }

      protected Task StubHandler(HttpContext context)
      {
         var serviceInfo = new ServiceInfo(new HttpContextAccessor {HttpContext = context});

         _baseAddress = serviceInfo.BaseAddress.ToString();

         return Task.CompletedTask;
      }

      [Test]
      public void should_return_status_code_200()
      {
         _response.StatusCode.ShouldBe(HttpStatusCode.OK);
      }

      [Test]
      public void should_compose_base_address()
      {
         _baseAddress.ShouldBe("http://localhost:5000");
      }

      [Test]
      public void should_write_start_metric()
      {
         var metric = (Counter) MetricSink.Metrics.First();

         Assert.That(metric.Name, Is.EqualTo("http_request_start"));
         Assert.That(metric.Dimensions["method"], Is.EqualTo("GET"));
         Assert.That(metric.Dimensions["path"], Is.EqualTo("/a/b/c"));
      }

      [Test]
      public void should_write_end_metric()
      {
         var metric = (Gauge) MetricSink.Metrics.Last();

         Assert.That(metric.Name, Is.EqualTo("http_request_end"));
         Assert.That(metric.Value, Is.GreaterThanOrEqualTo(0));
         Assert.That(metric.Dimensions["method"], Is.EqualTo("GET"));
         Assert.That(metric.Dimensions["path"], Is.EqualTo("/a/b/c"));
         Assert.That(metric.Dimensions["statusCode"], Is.EqualTo(200));
      }
   }
}
