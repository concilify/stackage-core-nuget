using System.Collections.Generic;
using System.Threading.Tasks;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Tests.Metrics
{
   public class StubMetricSink : IMetricSink
   {
      public IList<IMetric> Metrics { get; } = new List<IMetric>();

      public Task PushAsync(IMetric metric)
      {
         Metrics.Add(metric);

         return Task.CompletedTask;
      }
   }
}
