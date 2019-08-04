using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Polly;

namespace Stackage.Core.Tests.Polly.Timing
{
   public class action_throws_exception_with_execute_dimension
   {
      private Exception _exceptionToThrow;
      private Exception _exceptionThrown;
      private StubMetricSink _metricSink;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _exceptionToThrow = new Exception();
         _metricSink = new StubMetricSink();

         var policyFactory = new PolicyFactory();
         var timingPolicy = policyFactory.CreateAsyncTimingPolicy("bar", _metricSink);

         try
         {
            await timingPolicy.ExecuteAsync(async (context) =>
            {
               await Task.Delay(50);

               throw _exceptionToThrow;
            }, new Dictionary<string, object> {{"execute-key", "execute-value"}});
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
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"execute-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"execute-value"}));
      }

      [Test]
      public void should_write_end_metric()
      {
         var metric = (Gauge) _metricSink.Metrics.Last();

         Assert.That(metric.Name, Is.EqualTo("bar_end"));
         Assert.That(metric.Value, Is.InRange(25, 100));
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"execute-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"execute-value"}));
      }
   }
}
