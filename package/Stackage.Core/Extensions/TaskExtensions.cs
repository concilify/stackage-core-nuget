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

         using (cancellationToken.RegisterAction(s => s.TrySetResult(true), cancellationTaskSource))
         {
            if (task != await Task.WhenAny(task, cancellationTaskSource.Task))
            {
               throw new OperationCanceledException(cancellationToken);
            }

            await task;
         }
      }

      private static IDisposable RegisterAction(
         this CancellationToken cancellationToken,
         Action<TaskCompletionSource<bool>> action,
         TaskCompletionSource<bool> state)
      {
         return cancellationToken.Register(s =>
         {
            if (s != null)
            {
               action((TaskCompletionSource<bool>) s);
            }
         }, state);
      }
   }
}
