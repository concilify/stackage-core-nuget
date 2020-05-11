using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stackage.Core.Abstractions.StartupTasks;

namespace Stackage.Core.StartupTasks
{
   public class StartupTasksExecutor : IStartupTasksExecutor
   {
      private readonly IList<IStartupTask> _startupTasks;
      private readonly ILogger<StartupTasksExecutor> _logger;

      private int _outstandingTasks;
      private int _failedTasks;
      private Task _allTasksCompleted;

      public StartupTasksExecutor(
         IEnumerable<IStartupTask> startupTasks,
         ILogger<StartupTasksExecutor> logger)
      {
         _startupTasks = startupTasks.ToList();
         _logger = logger;
         _outstandingTasks = _startupTasks.Count;
      }

      public bool AllCompleteAndSuccessful => _outstandingTasks == 0 && _failedTasks == 0;

      public async Task<bool> AllCompleteAndSuccessfulAsync(CancellationToken cancellationToken)
      {
         if (_startupTasks.Count == 0)
         {
            return true;
         }

         // Handle race condition with BackgroundServices start order
         while (_allTasksCompleted == null)
         {
            await Task.Delay(100, cancellationToken);
         }

         try
         {
            await _allTasksCompleted;

            return _failedTasks == 0;
         }
         catch (Exception)
         {
            // Exception will be logged by ExecuteAsync
            return false;
         }
      }

      public async Task ExecuteAsync(CancellationToken cancellationToken)
      {
         if (_startupTasks.Count == 0)
         {
            return;
         }

         _allTasksCompleted = Task.WhenAll(_startupTasks.Select(c => ExecuteStartupTaskAsync(c, cancellationToken)));

         try
         {
            await _allTasksCompleted;
         }
         catch (Exception e)
         {
            _logger.LogError(e, $"Failed to wait for startup tasks to complete");
         }
      }

      private async Task ExecuteStartupTaskAsync(IStartupTask startupTask, CancellationToken cancellationToken)
      {
         _logger.LogInformation($"Startup task {startupTask.GetType().Name} executing...");

         try
         {
            await startupTask.ExecuteAsync(cancellationToken);

            RegisterCompletion(startupTask);
         }
         catch (Exception e)
         {
            RegisterFailure(startupTask, e);
         }
      }

      private void RegisterCompletion(IStartupTask startupTask)
      {
         Interlocked.Decrement(ref _outstandingTasks);

         _logger.LogInformation($"Startup task {startupTask.GetType().Name} completed");
      }

      private void RegisterFailure(IStartupTask startupTask, Exception e)
      {
         Interlocked.Increment(ref _failedTasks);
         Interlocked.Decrement(ref _outstandingTasks);

         _logger.LogError(e, $"Startup task {startupTask.GetType().Name} failed");
      }
   }
}
