using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Stackage.Core.Tests
{
   public class TestService
   {
      private readonly Action<IWebHostBuilder> _configureWebHostBuilder;
      private readonly Action<IConfigurationBuilder> _configureConfiguration;
      private readonly Action<IServiceCollection> _configureServices;
      private readonly Action<IApplicationBuilder> _configure;

      public TestService(
         Action<IWebHostBuilder> configureWebHostBuilder,
         Action<IConfigurationBuilder> configureConfiguration,
         Action<IServiceCollection> configureServices,
         Action<IApplicationBuilder> configure)
      {
         _configureWebHostBuilder = configureWebHostBuilder;
         _configureConfiguration = configureConfiguration;
         _configureServices = configureServices;
         _configure = configure;
      }

      public async Task<HttpResponseMessage> GetAsync(string uri, IDictionary<string, string> headers = null,
         CancellationToken cancellationToken = default)
      {
         using (var fakeServer = CreateServer())
         {
            return await GetAsync(fakeServer, uri, headers, cancellationToken);
         }
      }

      public async Task<HttpResponseMessage> GetAsync(TestServer server, string uri, IDictionary<string, string> headers = null,
         CancellationToken cancellationToken = default)
      {
         using (var httpClient = server.CreateClient())
         {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            AddHeadersToRequest(headers, request);

            return await httpClient.SendAsync(request, cancellationToken);
         }
      }

      public async Task<HttpResponseMessage> HeadAsync(string uri, IDictionary<string, string> headers = null)
      {
         using (var fakeServer = CreateServer())
         {
            return await HeadAsync(fakeServer, uri, headers);
         }
      }

      public async Task<HttpResponseMessage> HeadAsync(TestServer server, string uri, IDictionary<string, string> headers = null)
      {
         using (var httpClient = server.CreateClient())
         {
            var request = new HttpRequestMessage(HttpMethod.Head, uri);

            AddHeadersToRequest(headers, request);

            return await httpClient.SendAsync(request);
         }
      }

      public async Task<HttpResponseMessage> PostAsync(string uri, string body, IDictionary<string, string> headers = null,
         string bodyType = "application/json")
      {
         using (var fakeServer = CreateServer())
         {
            return await PostAsync(fakeServer, uri, body, headers, bodyType);
         }
      }

      public async Task<HttpResponseMessage> PostAsync(TestServer server, string uri, string body, IDictionary<string, string> headers = null,
         string bodyType = "application/json")
      {
         using (var client = server.CreateClient())
         {
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
               Content = new StringContent(body, Encoding.UTF8, bodyType)
            };

            AddHeadersToRequest(headers, request);

            return await client.SendAsync(request);
         }
      }

      public TestServer CreateServer()
      {
         var builder = new WebHostBuilder();

         _configureWebHostBuilder(builder);

         builder
            .ConfigureAppConfiguration(_configureConfiguration)
            .ConfigureServices(_configureServices)
            .Configure(_configure);

         var server = new TestServer(builder);
         return server;
      }

      private static void AddHeadersToRequest(IDictionary<string, string> headers, HttpRequestMessage request)
      {
         if (headers == null)
         {
            return;
         }

         foreach (var header in headers)
         {
            request.Headers.Add(header.Key, header.Value);
         }
      }
   }
}
