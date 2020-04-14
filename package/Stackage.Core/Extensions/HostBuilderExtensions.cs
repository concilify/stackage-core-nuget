using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Stackage.Core.Extensions
{
   public static class HostBuilderExtensions
   {
      public static IHostBuilder UseDefaultBuilder(this IHostBuilder hostBuilder, string[] args)
      {
         return hostBuilder
            .UseContentRoot(GetExeDirectory())
            .ConfigureHostConfiguration(builder =>
            {
               builder
                  .AddEnvironmentVariables(prefix: "DOTNET_")
                  .AddCommandLine(args);
            })
            .ConfigureAppConfiguration((context, builder) =>
            {
               var prefix = $"{context.HostingEnvironment.ApplicationName.Replace(".", "")}_";

               builder
                  .AddJsonFile("appsettings.json")
                  .AddDockerSecrets(prefix)
                  .AddEnvironmentVariables(prefix)
                  .AddCommandLine(args);
            })
            .UseDefaultServiceProvider((context, options) =>
            {
               var isDevelopment = context.HostingEnvironment.IsDevelopment();

               options.ValidateScopes = isDevelopment;
               options.ValidateOnBuild = isDevelopment;
            });
      }

      private static string GetExeDirectory()
      {
         return Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? Directory.GetCurrentDirectory());
      }
   }
}
