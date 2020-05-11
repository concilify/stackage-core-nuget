using System;
using System.Threading;
using System.Threading.Tasks;
using Stackage.Core.Abstractions.StartupTasks;

namespace Stackage.Core.Tests
{
   public class StubStartupTask : IStartupTask
   {
      public TimeSpan? Latency { get; set; }

      public Exception ThrowException { get; set; }

      public async Task ExecuteAsync(CancellationToken cancellationToken)
      {
         if (Latency != null)
         {
            await Task.Delay(Latency.Value, cancellationToken);
         }

         if (ThrowException != null)
         {
            throw ThrowException;
         }
      }
   }
}
