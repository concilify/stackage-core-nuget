using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Tests
{
   public class StubMetricSink : IMetricSink
   {
      private readonly ConcurrentQueue<IMetric> _metrics = new ConcurrentQueue<IMetric>();

      public IReadOnlyCollection<IMetric> Metrics => _metrics;

      public Task PushAsync(IMetric metric)
      {
         _metrics.Enqueue(metric);

         return Task.CompletedTask;
      }
   }
}
