using Microsoft.Extensions.Configuration;
using Stackage.Core.Configuration;

namespace Stackage.Core.Extensions
{
   public static class ConfigurationBuilderExtensions
   {
      public static IConfigurationBuilder AddDockerSecrets(this IConfigurationBuilder configurationBuilder)
      {
         configurationBuilder.Add(new DockerSecretsConfigurationSource());
         return configurationBuilder;
      }

      public static IConfigurationBuilder AddDockerSecrets(this IConfigurationBuilder configurationBuilder, string prefix)
      {
         configurationBuilder.Add(new DockerSecretsConfigurationSource {Prefix = prefix});
         return configurationBuilder;
      }
   }
}
