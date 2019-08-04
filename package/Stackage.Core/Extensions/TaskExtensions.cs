using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stackage.Core.Extensions
{
   public static class TaskExtensions
   {
      public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
      {
         var cancellationTaskSource = new TaskCompletionSource<bool>();

         using (cancellationToken.Register(s => s.TrySetResult(true), cancellationTaskSource))
         {
            if (task != await Task.WhenAny(task, cancellationTaskSource.Task))
            {
               throw new OperationCanceledException(cancellationToken);
            }

            await task;
         }
      }

      private static IDisposable Register<T>(this CancellationToken cancellationToken, Action<T> action, T state)
      {
         return cancellationToken.Register(s => { action((T) s); }, state);
      }
   }
}
