using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shouldly;

namespace Stackage.Core.Tests.DefaultMiddleware.ForwardHeaders.ForwardHeadersTrue
{
   public class without_headers : middleware_scenario
   {
      private string _scheme;
      private HostString _host;
      private IPAddress _remoteIpAddress;
      private HttpResponseMessage _response;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _response = await TestService.GetAsync("http://localhost:5000/get");
      }

      protected override void ConfigureConfiguration(IConfigurationBuilder configurationBuilder)
      {
         base.ConfigureConfiguration(configurationBuilder);

         configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
         {
            {"STACKAGE:FORWARDHEADERS", "true"}
         });
      }

      protected override void Configure(IApplicationBuilder app)
      {
         base.Configure(app);

         app.UseMiddleware<StubResponseMiddleware>(new StubResponseOptions(HttpStatusCode.OK, "content") {Handler = StubHandler});
      }

      protected Task StubHandler(HttpContext context)
      {
         _scheme = context.Request.Scheme;
         _host = context.Request.Host;
         _remoteIpAddress = context.Connection.RemoteIpAddress;

         return Task.CompletedTask;
      }

      [Test]
      public void should_return_status_code_200()
      {
         _response.StatusCode.ShouldBe(HttpStatusCode.OK);
      }

      [Test]
      public void should_make_scheme_available()
      {
         _scheme.ShouldBe("http");
      }

      [Test]
      public void should_make_host_available()
      {
         _host.ToString().ShouldBe("localhost:5000");
      }

      [Test]
      public void should_make_remote_ip_address_available()
      {
         _remoteIpAddress.ShouldBeNull();
      }
   }
}
