using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Stackage.Core.Tests
{
   public class StubLogger<T> : ILogger<T>
   {
      public IList<Entry> Entries { get; } = new List<Entry>();

      public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
         Func<TState, Exception, string> formatter)
      {
         var entry = new Entry
         {
            LogLevel = logLevel,
            Message = formatter(state, exception)
         };

         if (state is IReadOnlyList<KeyValuePair<string, object>> values)
         {
            const string originalMessageKey = "{OriginalFormat}";

            var (_, originalMessage) = values.SingleOrDefault(c => c.Key == originalMessageKey);

            entry.Values = new Dictionary<string, object>(values.Where(c => c.Key != originalMessageKey));
            entry.OriginalMessage = originalMessage as string;
         }

         Entries.Add(entry);
      }

      public bool IsEnabled(LogLevel logLevel)
      {
         return true;
      }

      public IDisposable BeginScope<TState>(TState state)
      {
         return null;
      }

      public class Entry
      {
         public LogLevel LogLevel { get; set; }

         public string Message { get; set; }

         public string OriginalMessage { get; set; }

         public IDictionary<string, object> Values { get; set; }

         public Exception Exception { get; set; }
      }
   }
}
