using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Stackage.Core.Tests.DefaultMiddleware.Health
{
   public class with_service_info : health_scenario
   {
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _response = await TestService.GetAsync("/health");
         _content = await _response.Content.ReadAsStringAsync();
      }

      protected override void ConfigureDependencies()
      {
         base.ConfigureDependencies();

         A.CallTo(() => ServiceInfo.Service).Returns("alt-service");
         A.CallTo(() => ServiceInfo.Version).Returns("alt-version");
         A.CallTo(() => ServiceInfo.Host).Returns("alt-host");
      }

      [Test]
      public void should_return_content()
      {
         var response = JObject.Parse(_content);

         var expectedResponse = new JObject
         {
            ["service"] = "alt-service",
            ["version"] = "alt-version",
            ["host"] = "alt-host"
         };

         response.Should().ContainSubtree(expectedResponse);
      }
   }
}
