using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Stackage.Core.Configuration
{
   // TODO: Add some tests for this
   public class DockerSecretsConfigurationProvider : ConfigurationProvider
   {
      private const string SecretsPath = "/run/secrets";
      private const string KeyDelimiter = "__";

      private readonly string _prefix;

      public DockerSecretsConfigurationProvider(string prefix)
      {
         _prefix = prefix;
      }

      public override void Load()
      {
         if (!Directory.Exists(SecretsPath))
         {
            return;
         }

         var filteredSecrets = Directory.EnumerateFiles(SecretsPath)
            .Where(File.Exists)
            .Select(path => new {Path = path, Key = DeriveKeyFrom(path)})
            .Where(secret => secret.Key.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase));
         ;

         foreach (var secret in filteredSecrets)
         {
            var key = secret.Key.Substring(_prefix.Length);
            var value = File.ReadAllText(secret.Path);

            Data.Add(key, value);
         }
      }

      private static string DeriveKeyFrom(string path)
      {
         return Path.GetFileName(path).Replace(KeyDelimiter, ConfigurationPath.KeyDelimiter);
      }
   }
}
