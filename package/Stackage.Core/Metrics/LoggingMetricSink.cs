using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Metrics
{
   public class LoggingMetricSink : IMetricSink
   {
      private readonly ILogger<LoggingMetricSink> _logger;

      public LoggingMetricSink(ILogger<LoggingMetricSink> logger)
      {
         _logger = logger;
      }

      public Task PushAsync(IMetric metric)
      {
         _logger.LogInformation("{@metric}", metric);

         return Task.CompletedTask;
      }
   }
}
