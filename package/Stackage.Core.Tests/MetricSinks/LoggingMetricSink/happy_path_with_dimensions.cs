using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Tests.MetricSinks.LoggingMetricSink
{
   public class happy_path_with_dimensions
   {
      private StubLogger<Core.MetricSinks.LoggingMetricSink> _logger;

      [OneTimeSetUp]
      public async Task setup_once_before_all_tests()
      {
         _logger = new StubLogger<Core.MetricSinks.LoggingMetricSink>();

         var loggingMetricSink = new Core.MetricSinks.LoggingMetricSink(_logger);

         var metric = new Counter("foo") {Dimensions = new Dictionary<string, object> {{"a", 1}, {"b", 2}}};
         await loggingMetricSink.PushAsync(metric);
      }

      [Test]
      public void should_pass_metric_as_argument_to_logger()
      {
         var metric = (Counter) _logger.Entries.Single().Values["@metric"];

         Assert.That(metric.Name, Is.EqualTo("foo"));
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"a", "b"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {1, 2}));
      }
   }
}
