using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Metrics
{
   public class TimingBlockFactory : ITimingBlockFactory
   {
      private readonly IMetricSink _metricSink;

      public TimingBlockFactory(IMetricSink metricSink)
      {
         _metricSink = metricSink;
      }

      public ITimingBlock Create(string name)
      {
         return new TimingBlock(name, _metricSink);
      }
   }
}
