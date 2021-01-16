using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.MetricSinks;

namespace Stackage.Core.Tests.MetricSinks
{
   public class PrometheusOptionsTests
   {
      private PrometheusOptions _prometheusOptions;

      [OneTimeSetUp]
      public void setup_once_before_all_tests()
      {
         var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
            .AddJsonFile("metrics-tests.appsettings.json")
            .Build();

         var stackageConfiguration = configuration.GetSection("stackage");

         var services = new ServiceCollection();
         services.Configure<PrometheusOptions>(stackageConfiguration.GetSection("prometheus"));

         var serviceProvider = services.BuildServiceProvider();

         _prometheusOptions = serviceProvider.GetRequiredService<IOptions<PrometheusOptions>>().Value;
      }

      [Test]
      public void parses_default_buckets()
      {
         _prometheusOptions.Buckets.ShouldBe(new[] {10d, 30d, 100d, 300d, 1000d});
      }

      [Test]
      public void parses_four_metrics()
      {
         _prometheusOptions.Metrics.Count.ShouldBe(4);
      }

      [Test]
      public void parses_http_request_start_metric()
      {
         _prometheusOptions.Metrics[0].Name.ShouldBe("http_request_start");
         _prometheusOptions.Metrics[0].Type.ShouldBe("Counter");
         _prometheusOptions.Metrics[0].Description.ShouldBe("HTTP Server Requests (Count)");
         _prometheusOptions.Metrics[0].Labels.ShouldBe(new[] {"method", "path"});
      }

      [Test]
      public void parses_http_request_end_metric()
      {
         _prometheusOptions.Metrics[1].Name.ShouldBe("http_request_end");
         _prometheusOptions.Metrics[1].Type.ShouldBe("Histogram");
         _prometheusOptions.Metrics[1].Description.ShouldBe("HTTP Server Requests (Duration ms)");
         _prometheusOptions.Metrics[1].Labels.ShouldBe(new[] {"method", "path", "statusCode", "exception"});
      }

      [Test]
      public void parses_db_query_start_metric()
      {
         _prometheusOptions.Metrics[2].Name.ShouldBe("db_query_start");
         _prometheusOptions.Metrics[2].Type.ShouldBe("Counter");
         _prometheusOptions.Metrics[2].Description.ShouldBe("Database Queries (Count)");
         _prometheusOptions.Metrics[2].Labels.ShouldBe(new[] {"type"});
      }

      [Test]
      public void parses_db_query_end_metric()
      {
         _prometheusOptions.Metrics[3].Name.ShouldBe("db_query_end");
         _prometheusOptions.Metrics[3].Type.ShouldBe("Histogram");
         _prometheusOptions.Metrics[3].Description.ShouldBe("Database Queries (Duration ms)");
         _prometheusOptions.Metrics[3].Labels.ShouldBe(new[] {"type"});
         _prometheusOptions.Metrics[3].Buckets.ShouldBe(new[] {1d, 3d, 10d, 30d, 100d});
      }
   }
}
