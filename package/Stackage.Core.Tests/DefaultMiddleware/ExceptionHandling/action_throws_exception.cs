using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Tests.DefaultMiddleware.ExceptionHandling
{
   public class action_throws_exception : middleware_scenario
   {
      private Exception _exceptionToThrow;
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

         A.CallTo(() => GuidGenerator.Generate()).ReturnsNextFromSequence("abc123def456");
      }

      protected override void Configure(IApplicationBuilder app)
      {
         base.Configure(app);

         _exceptionToThrow = new Exception();
         app.UseMiddleware<StubResponseMiddleware>(new StubResponseOptions {ThrowException = _exceptionToThrow});
      }

      [Test]
      public void should_return_status_code_500()
      {
         _response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
      }

      [Test]
      public void should_return_json_content_with_token()
      {
         _content.ShouldBe("{\"message\":\"Internal Server Error\",\"token\":\"ABC123DE\"}");
      }

      [Test]
      public void should_return_content_type_json()
      {
         _response.Content.Headers.ContentType.MediaType.ShouldBe("application/json");
      }

      [Test]
      public void should_log_a_single_message()
      {
         Logger.Entries.Count.ShouldBe(1);
      }

      [Test]
      public void should_log_error_message_with_token()
      {
         Logger.Entries[0].LogLevel.ShouldBe(LogLevel.Error);
         Logger.Entries[0].Message.ShouldBe("An unexpected exception has occurred (token=ABC123DE)");
      }

      [Test]
      public void should_write_two_metrics()
      {
         Assert.That(MetricSink.Metrics.Count, Is.EqualTo(2));
      }

      [Test]
      public void should_write_start_metric()
      {
         var metric = (Counter) MetricSink.Metrics.First();

         Assert.That(metric.Name, Is.EqualTo("http_request_start"));
         Assert.That(metric.Dimensions["method"], Is.EqualTo("GET"));
         Assert.That(metric.Dimensions["path"], Is.EqualTo("/get"));
      }

      [Test]
      public void should_write_end_metric()
      {
         var metric = (Gauge) MetricSink.Metrics.Last();

         Assert.That(metric.Name, Is.EqualTo("http_request_end"));
         Assert.That(metric.Value, Is.GreaterThanOrEqualTo(0));
         Assert.That(metric.Dimensions["method"], Is.EqualTo("GET"));
         Assert.That(metric.Dimensions["path"], Is.EqualTo("/get"));
         Assert.That(metric.Dimensions["statusCode"], Is.EqualTo(500));
      }
   }
}
