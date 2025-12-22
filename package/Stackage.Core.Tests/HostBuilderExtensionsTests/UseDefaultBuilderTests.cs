using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Stackage.Core.Extensions;

namespace Stackage.Core.Tests.HostBuilderExtensionsTests
{
   public class UseDefaultBuilderTests
   {
      private string _testDirectory;
      private string _originalDirectory;

      [SetUp]
      public void SetUp()
      {
         // UseDefaultBuilder uses the exe directory, so we need to use the actual output directory
         _testDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? Directory.GetCurrentDirectory())!;
         _originalDirectory = Directory.GetCurrentDirectory();
         
         // Change to test directory for tests
         Directory.SetCurrentDirectory(_testDirectory);
      }

      [TearDown]
      public void TearDown()
      {
         // Restore original directory
         Directory.SetCurrentDirectory(_originalDirectory);

         // Clean up test files created in the exe directory
         CleanupTestFile("appsettings.json");
         CleanupTestFile("appsettings.Development.json");
         CleanupTestFile("appsettings.Production.json");
         CleanupTestFile("appsettings.Staging.json");

         // Clear environment variables that might have been set during tests
         Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
         Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
      }

      private void CleanupTestFile(string filename)
      {
         var filePath = Path.Combine(_testDirectory, filename);
         if (File.Exists(filePath))
         {
            File.Delete(filePath);
         }
      }

      private static string GetApplicationNamePrefix()
      {
         // Get the application name prefix by creating a temporary host
         // This is needed because the prefix is based on the application name with dots removed
         var tempHost = new HostBuilder()
            .UseDefaultBuilder(Array.Empty<string>())
            .ConfigureServices((context, services) => { })
            .Build();
         
         var hostEnvironment = tempHost.Services.GetRequiredService<IHostEnvironment>();
         var prefix = $"{hostEnvironment.ApplicationName.Replace(".", "")}_";
         tempHost.Dispose();
         
         return prefix;
      }

      [Test]
      public void should_load_base_appsettings_json()
      {
         // Arrange
         var appSettingsPath = Path.Combine(_testDirectory, "appsettings.json");
         File.WriteAllText(appSettingsPath, @"{""TestKey"": ""BaseValue""}");

         // Act
         var host = new HostBuilder()
            .UseDefaultBuilder(Array.Empty<string>())
            .ConfigureServices((context, services) => { })
            .Build();

         var configuration = host.Services.GetRequiredService<IConfiguration>();

         // Assert
         Assert.That(configuration["TestKey"], Is.EqualTo("BaseValue"));
      }

      [Test]
      public void should_load_environment_specific_appsettings_json()
      {
         // Arrange
         var appSettingsPath = Path.Combine(_testDirectory, "appsettings.json");
         var appSettingsDevelopmentPath = Path.Combine(_testDirectory, "appsettings.Development.json");
         
         File.WriteAllText(appSettingsPath, @"{""TestKey"": ""BaseValue"", ""OnlyInBase"": ""Base""}");
         File.WriteAllText(appSettingsDevelopmentPath, @"{""TestKey"": ""DevelopmentValue""}");

         Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");

         // Act
         var host = new HostBuilder()
            .UseDefaultBuilder(Array.Empty<string>())
            .ConfigureServices((context, services) => { })
            .Build();

         var configuration = host.Services.GetRequiredService<IConfiguration>();

         // Assert
         Assert.That(configuration["TestKey"], Is.EqualTo("DevelopmentValue"), "Environment-specific setting should override base setting");
         Assert.That(configuration["OnlyInBase"], Is.EqualTo("Base"), "Base settings should still be available");
      }

      [Test]
      public void should_load_production_specific_appsettings_json()
      {
         // Arrange
         var appSettingsPath = Path.Combine(_testDirectory, "appsettings.json");
         var appSettingsProductionPath = Path.Combine(_testDirectory, "appsettings.Production.json");
         
         File.WriteAllText(appSettingsPath, @"{""TestKey"": ""BaseValue""}");
         File.WriteAllText(appSettingsProductionPath, @"{""TestKey"": ""ProductionValue""}");

         Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");

         // Act
         var host = new HostBuilder()
            .UseDefaultBuilder(Array.Empty<string>())
            .ConfigureServices((context, services) => { })
            .Build();

         var configuration = host.Services.GetRequiredService<IConfiguration>();

         // Assert
         Assert.That(configuration["TestKey"], Is.EqualTo("ProductionValue"));
      }

      [Test]
      public void should_handle_missing_environment_specific_appsettings()
      {
         // Arrange
         var appSettingsPath = Path.Combine(_testDirectory, "appsettings.json");
         File.WriteAllText(appSettingsPath, @"{""TestKey"": ""BaseValue""}");

         Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Staging");

         // Act
         var host = new HostBuilder()
            .UseDefaultBuilder(Array.Empty<string>())
            .ConfigureServices((context, services) => { })
            .Build();

         var configuration = host.Services.GetRequiredService<IConfiguration>();

         // Assert - should fall back to base settings
         Assert.That(configuration["TestKey"], Is.EqualTo("BaseValue"));
      }

      [Test]
      public void should_load_environment_variables_with_app_prefix()
      {
         // Arrange
         var appSettingsPath = Path.Combine(_testDirectory, "appsettings.json");
         File.WriteAllText(appSettingsPath, @"{""Logging"": {""LogLevel"": {""Default"": ""Information""}}}");

         var prefix = GetApplicationNamePrefix();
         var envVarName = $"{prefix}TestKey";
         Environment.SetEnvironmentVariable(envVarName, "EnvVarValue");

         // Act
         var host = new HostBuilder()
            .UseDefaultBuilder(Array.Empty<string>())
            .ConfigureServices((context, services) => { })
            .Build();

         var configuration = host.Services.GetRequiredService<IConfiguration>();

         // Assert
         Assert.That(configuration["TestKey"], Is.EqualTo("EnvVarValue"));

         // Cleanup
         Environment.SetEnvironmentVariable(envVarName, null);
      }

      [Test]
      public void should_load_dotnet_environment_variables()
      {
         // Arrange
         Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");

         // Act
         var host = new HostBuilder()
            .UseDefaultBuilder(Array.Empty<string>())
            .ConfigureServices((context, services) => { })
            .Build();

         var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();

         // Assert
         Assert.That(hostEnvironment.EnvironmentName, Is.EqualTo("Testing"));
      }

      [Test]
      public void should_load_command_line_arguments()
      {
         // Arrange
         var appSettingsPath = Path.Combine(_testDirectory, "appsettings.json");
         File.WriteAllText(appSettingsPath, @"{""TestKey"": ""BaseValue""}");

         var args = new[] { "--TestKey=CommandLineValue", "--AnotherKey=AnotherValue" };

         // Act
         var host = new HostBuilder()
            .UseDefaultBuilder(args)
            .ConfigureServices((context, services) => { })
            .Build();

         var configuration = host.Services.GetRequiredService<IConfiguration>();

         // Assert
         Assert.That(configuration["TestKey"], Is.EqualTo("CommandLineValue"), "Command line args should override file settings");
         Assert.That(configuration["AnotherKey"], Is.EqualTo("AnotherValue"), "Command line args should be available");
      }

      [Test]
      public void should_prioritize_configuration_sources_correctly()
      {
         // Arrange
         var appSettingsPath = Path.Combine(_testDirectory, "appsettings.json");
         var appSettingsDevelopmentPath = Path.Combine(_testDirectory, "appsettings.Development.json");
         
         File.WriteAllText(appSettingsPath, @"{""TestKey"": ""BaseValue""}");
         File.WriteAllText(appSettingsDevelopmentPath, @"{""TestKey"": ""DevelopmentValue""}");

         Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");

         var prefix = GetApplicationNamePrefix();
         var envVarName = $"{prefix}TestKey";
         Environment.SetEnvironmentVariable(envVarName, "EnvVarValue");
         
         var args = new[] { "--TestKey=CommandLineValue" };

         // Act
         var host = new HostBuilder()
            .UseDefaultBuilder(args)
            .ConfigureServices((context, services) => { })
            .Build();

         var configuration = host.Services.GetRequiredService<IConfiguration>();

         // Assert - Command line should have highest priority
         Assert.That(configuration["TestKey"], Is.EqualTo("CommandLineValue"), 
            "Command line args should have highest priority");

         // Cleanup
         Environment.SetEnvironmentVariable(envVarName, null);
      }

      [Test]
      public void should_validate_scopes_in_development()
      {
         // Arrange
         Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");

         // Act
         var host = new HostBuilder()
            .UseDefaultBuilder(Array.Empty<string>())
            .ConfigureServices((context, services) =>
            {
               // Add a scoped service to test validation
               services.AddScoped<TestScopedService>();
            })
            .Build();

         // Assert - Attempting to resolve a scoped service from root provider in Development should throw
         Assert.Throws<InvalidOperationException>(() =>
         {
            host.Services.GetRequiredService<TestScopedService>();
         });
      }

      [Test]
      public void should_not_validate_scopes_in_production()
      {
         // Arrange
         Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");

         // Act
         var host = new HostBuilder()
            .UseDefaultBuilder(Array.Empty<string>())
            .ConfigureServices((context, services) =>
            {
               // Add a scoped service to test validation
               services.AddScoped<TestScopedService>();
            })
            .Build();

         // Assert - In Production, resolving scoped service from root provider should not throw
         Assert.DoesNotThrow(() =>
         {
            host.Services.GetRequiredService<TestScopedService>();
         });
      }

      // Test service used to verify scope validation behavior
      private class TestScopedService
      {
      }
   }
}
