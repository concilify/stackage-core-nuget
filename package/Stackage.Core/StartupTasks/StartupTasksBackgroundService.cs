using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Stackage.Core.Abstractions.StartupTasks;

namespace Stackage.Core.StartupTasks
{
   public class StartupTasksBackgroundService : BackgroundService
   {
      private readonly IStartupTasksExecutor _startupTasksExecutor;

      public StartupTasksBackgroundService(IStartupTasksExecutor startupTasksExecutor)
      {
         _startupTasksExecutor = startupTasksExecutor;
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
         await _startupTasksExecutor.ExecuteAsync(stoppingToken);
      }
   }
}
