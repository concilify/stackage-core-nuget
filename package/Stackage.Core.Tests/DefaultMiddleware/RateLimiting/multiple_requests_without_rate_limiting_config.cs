using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using Shouldly;

namespace Stackage.Core.Tests.DefaultMiddleware.RateLimiting
{
   public class multiple_requests_without_rate_limiting_config : middleware_scenario
   {
      private HttpResponseMessage[] _responses;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         using (var server = TestService.CreateServer())
         {
            var gets = Enumerable.Range(0, 100).Select(_ => TestService.GetAsync(server, "/get")).ToArray();

            await Task.WhenAll(gets);

            _responses = gets.Select(x => x.Result).ToArray();
         }
      }

      protected override void Configure(IApplicationBuilder app)
      {
         base.Configure(app);

         app.UseMiddleware<StubResponseMiddleware>(new StubResponseOptions(HttpStatusCode.OK, ""));
      }

      [Test]
      public void should_all_return_status_code_200()
      {
         _responses.ShouldAllBe(x => x.StatusCode == HttpStatusCode.OK);
      }

      [Test]
      public void should_not_log_a_message()
      {
         Logger.Entries.Count.ShouldBe(0);
      }

      [Test]
      public void should_write_two_hundred_metrics()
      {
         Assert.That(MetricSink.Metrics.Count, Is.EqualTo(200));
      }

      [Test]
      public void end_metrics_should_all_have_status_code_200()
      {
         MetricSink.Metrics.Where(x => x.Name == "http_request_end").ShouldAllBe(x => (int) x.Dimensions["statusCode"] == 200);
      }
   }
}
