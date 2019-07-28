using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Metrics;

namespace Stackage.Core.Tests.Metrics.TimingMetricsScenarios
{
   public class action_throws_exception_with_dimension
   {
      private Exception _exceptionToThrow;
      private Exception _exceptionThrown;
      private StubMetricSink _metricSink;

      [OneTimeSetUp]
      public async Task setup_once_before_all_tests()
      {
         _exceptionToThrow = new Exception();
         _metricSink = new StubMetricSink();

         var timingBlockFactory = new TimingBlockFactory(_metricSink);
         var timingBlock = timingBlockFactory.Create("bar");

         timingBlock.Dimensions.Add("the-key", "the-value");
         try
         {
            await timingBlock.ExecuteAsync(async () =>
            {
               await Task.Delay(50);

               throw _exceptionToThrow;
            });
         }
         catch (Exception e)
         {
            _exceptionThrown = e;
         }
      }

      [Test]
      public void should_throw_exception()
      {
         Assert.That(_exceptionThrown, Is.SameAs(_exceptionToThrow));
      }

      [Test]
      public void should_write_two_metrics()
      {
         Assert.That(_metricSink.Metrics.Count, Is.EqualTo(2));
      }

      [Test]
      public void should_write_start_metric()
      {
         var metric = (Counter) _metricSink.Metrics.First();

         Assert.That(metric.Name, Is.EqualTo("bar_start"));
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"the-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"the-value"}));
      }

      [Test]
      public void should_write_end_metric()
      {
         var metric = (Gauge) _metricSink.Metrics.Last();

         Assert.That(metric.Name, Is.EqualTo("bar_end"));
         Assert.That(metric.Value, Is.InRange(25, 100));
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"the-key", "exception"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"the-value", "System.Exception"}));
      }
   }
}
