using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Options;
using Counter = Prometheus.Counter;
using Gauge = Stackage.Core.Abstractions.Metrics.Gauge;

namespace Stackage.Core.MetricSinks
{
   public class PrometheusMetricSink : BackgroundService, IMetricSink
   {
      private readonly IDictionary<string, Action<IMetric>> _pushers = new Dictionary<string, Action<IMetric>>();
      private readonly IDictionary<string, IList<Sanitiser>> _sanitisers = new Dictionary<string, IList<Sanitiser>>();
      private readonly PrometheusOptions _options;
      private readonly BlockingCollection<IMetric> _queue;
      private readonly ILogger<PrometheusMetricSink> _logger;

      public PrometheusMetricSink(
         IOptions<PrometheusOptions> options,
         ILogger<PrometheusMetricSink> logger)
      {
         if (options == null) throw new ArgumentNullException(nameof(options));

         _options = options.Value;

         _queue = new BlockingCollection<IMetric>(_options.BufferCapacity);
         _logger = logger;

         InitialisePushers();
         InitialiseSanitisers();
      }

      public Task PushAsync(IMetric metric)
      {
         _logger.LogDebug("{@metric}", metric);

         if (!_queue.TryAdd(metric))
         {
            _logger.LogWarning("Failed to queue metric {@metric}", metric);
         }

         return Task.CompletedTask;
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
         // Prevent GetConsumingEnumerable from blocking startup
         await Task.Yield();

         foreach (var metric in _queue.GetConsumingEnumerable(stoppingToken))
         {
            try
            {
               if (_pushers.TryGetValue(metric.Name, out var push))
               {
                  push(metric);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Failed to push metric {@metric}", metric);
            }
         }
      }

      private void InitialisePushers()
      {
         foreach (var metric in _options.Metrics)
         {
            if (metric.Type == "Counter")
            {
               var counter = Metrics.CreateCounter(metric.Name, metric.Description ?? string.Empty,
                  new CounterConfiguration
                  {
                     LabelNames = metric.Labels
                  });

               _pushers.Add(metric.Name, m => ApplyToCounter(counter, m));
            }
            else if (metric.Type == "Histogram")
            {
               var histogram = Metrics.CreateHistogram(metric.Name, metric.Description ?? string.Empty,
                  new HistogramConfiguration
                  {
                     LabelNames = metric.Labels,
                     Buckets = metric.Buckets ?? _options.Buckets ?? new[] {10d, 30d, 100d}
                  });

               _pushers.Add(metric.Name, m => ApplyToHistogram(histogram, m));
            }
         }
      }

      private void InitialiseSanitisers()
      {
         foreach (var metric in _options.Metrics)
         {
            if (metric.Sanitisers == null)
            {
               continue;
            }

            foreach (var sanitiser in metric.Sanitisers)
            {
               if (!metric.Labels.Contains(sanitiser.Label))
               {
                  continue;
               }

               var nameAndLabel = $"{metric.Name}:{sanitiser.Label}";

               if (!_sanitisers.TryGetValue(nameAndLabel, out var patterns))
               {
                  patterns = new List<Sanitiser>();

                  _sanitisers.Add(nameAndLabel, patterns);
               }

               if (sanitiser.Literal != null)
               {
                  patterns.Add(Sanitiser.ForLiteral(sanitiser.Literal, sanitiser.Value));
               }
               else if (sanitiser.Pattern != null)
               {
                  patterns.Add(Sanitiser.ForPattern(sanitiser.Pattern, sanitiser.Value));
               }
            }
         }
      }

      private string[] ExtractLabels(ICollector collector, IMetric metric)
      {
         string ExtractLabel(string name)
         {
            if (metric.Dimensions.TryGetValue(name, out var value))
            {
               var labelValue = value.ToString() ?? string.Empty;

               if (_sanitisers.TryGetValue($"{metric.Name}:{name}", out var sanitisers))
               {
                  var pattern = sanitisers.FirstOrDefault(c => c.Match(labelValue));

                  if (pattern == null)
                  {
                     _logger.LogWarning($"Value {value} of label {name} metric {metric.Name} cannot be sanitised");

                     return _options.SanitiserFallback;
                  }

                  return pattern.Value;
               }

               return labelValue;
            }

            return string.Empty;
         }

         return collector.LabelNames.Select(ExtractLabel).ToArray();
      }

      private void ApplyToCounter(Counter counter, IMetric metric)
      {
         counter.WithLabels(ExtractLabels(counter, metric)).Inc();
      }

      private void ApplyToHistogram(Histogram histogram, IMetric metric)
      {
         histogram.WithLabels(ExtractLabels(histogram, metric)).Observe(((Gauge) metric).Value);
      }

      private class Sanitiser
      {
         private readonly Func<string, bool> _match;

         private Sanitiser(Func<string, bool> match, string value)
         {
            _match = match;
            Value = value;
         }

         public string Value { get; }

         public bool Match(string value) => _match(value);

         public static Sanitiser ForLiteral(string literal, string value)
         {
            return new(v => v == literal, value);
         }

         public static Sanitiser ForPattern(string pattern, string value)
         {
            var regex = new Regex(pattern);

            return new(v => regex.IsMatch(v), value);
         }
      }
   }
}
