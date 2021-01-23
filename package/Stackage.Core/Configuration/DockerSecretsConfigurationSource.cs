using Microsoft.Extensions.Configuration;

namespace Stackage.Core.Configuration
{
   public class DockerSecretsConfigurationSource : IConfigurationSource
   {
      public string Prefix { get; set; } = string.Empty;

      public IConfigurationProvider Build(IConfigurationBuilder builder)
      {
         return new DockerSecretsConfigurationProvider(Prefix);
      }
   }
}
