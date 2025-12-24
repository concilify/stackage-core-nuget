using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Stackage.Core.Extensions;

namespace Stackage.Core.Tests.HostBuilderExtensionsTests;

public class UseDefaultBuilderTests
{
   private string _testDirectory;

   [SetUp]
   public void setup_before_each_test()
   {
      // Use the application base directory where the test binaries are located.
      // Using Assembly.GetEntryAssembly() points to the test runner (e.g., Rider's runner in Program Files),
      // which causes UnauthorizedAccessException when trying to write files.
      _testDirectory = AppDomain.CurrentDomain.BaseDirectory;
   }

   [TearDown]
   public void teardown_after_each_test()
   {
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
      using var host = new HostBuilder()
         .UseDefaultBuilder([])
         .Build();

      var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();
      return $"{hostEnvironment.ApplicationName.Replace(".", "")}_";
   }

   [Test]
   public void should_load_base_appsettings_json()
   {
      var appSettingsPath = Path.Combine(_testDirectory, "appsettings.json");
      File.WriteAllText(appSettingsPath, @"{""TestKey"": ""BaseValue""}");

      using var host = new HostBuilder()
         .UseDefaultBuilder([])
         .Build();

      var configuration = host.Services.GetRequiredService<IConfiguration>();

      Assert.That(configuration["TestKey"], Is.EqualTo("BaseValue"));
   }

   [Test]
   public void should_load_environment_specific_appsettings_json()
   {
      var appSettingsPath = Path.Combine(_testDirectory, "appsettings.json");
      var appSettingsDevelopmentPath = Path.Combine(_testDirectory, "appsettings.Development.json");

      File.WriteAllText(appSettingsPath, @"{""TestKey"": ""BaseValue"", ""OnlyInBase"": ""Base""}");
      File.WriteAllText(appSettingsDevelopmentPath, @"{""TestKey"": ""DevelopmentValue""}");

      Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");

      using var host = new HostBuilder()
         .UseDefaultBuilder([])
         .Build();

      var configuration = host.Services.GetRequiredService<IConfiguration>();

      Assert.That(configuration["TestKey"], Is.EqualTo("DevelopmentValue"), "Environment-specific setting should override base setting");
      Assert.That(configuration["OnlyInBase"], Is.EqualTo("Base"), "Base settings should still be available");
   }

   [Test]
   public void should_load_production_specific_appsettings_json()
   {
      var appSettingsPath = Path.Combine(_testDirectory, "appsettings.json");
      var appSettingsProductionPath = Path.Combine(_testDirectory, "appsettings.Production.json");

      File.WriteAllText(appSettingsPath, @"{""TestKey"": ""BaseValue""}");
      File.WriteAllText(appSettingsProductionPath, @"{""TestKey"": ""ProductionValue""}");

      Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");

      using var host = new HostBuilder()
         .UseDefaultBuilder([])
         .Build();

      var configuration = host.Services.GetRequiredService<IConfiguration>();

      Assert.That(configuration["TestKey"], Is.EqualTo("ProductionValue"));
   }

   [Test]
   public void should_handle_missing_environment_specific_appsettings()
   {
      var appSettingsPath = Path.Combine(_testDirectory, "appsettings.json");
      File.WriteAllText(appSettingsPath, @"{""TestKey"": ""BaseValue""}");

      Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Staging");

      using var host = new HostBuilder()
         .UseDefaultBuilder([])
         .Build();

      var configuration = host.Services.GetRequiredService<IConfiguration>();

      Assert.That(configuration["TestKey"], Is.EqualTo("BaseValue"));
   }

   [Test]
   public void should_load_environment_variables_with_app_prefix()
   {
      var prefix = GetApplicationNamePrefix();
      var envVarName = $"{prefix}TestKey";
      Environment.SetEnvironmentVariable(envVarName, "EnvVarValue");

      using var host = new HostBuilder()
         .UseDefaultBuilder([])
         .Build();

      var configuration = host.Services.GetRequiredService<IConfiguration>();

      Assert.That(configuration["TestKey"], Is.EqualTo("EnvVarValue"));

      Environment.SetEnvironmentVariable(envVarName, null);
   }

   [Test]
   public void should_load_dotnet_environment_variables()
   {
      Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");

      using var host = new HostBuilder()
         .UseDefaultBuilder([])
         .Build();

      var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();

      Assert.That(hostEnvironment.EnvironmentName, Is.EqualTo("Testing"));
   }

   [Test]
   public void should_load_command_line_arguments()
   {
      var appSettingsPath = Path.Combine(_testDirectory, "appsettings.json");
      File.WriteAllText(appSettingsPath, @"{""TestKey"": ""BaseValue""}");

      var args = new[] { "--TestKey=CommandLineValue", "--AnotherKey=AnotherValue" };

      using var host = new HostBuilder()
         .UseDefaultBuilder(args)
         .Build();

      var configuration = host.Services.GetRequiredService<IConfiguration>();

      Assert.That(configuration["TestKey"], Is.EqualTo("CommandLineValue"), "Command line args should override file settings");
      Assert.That(configuration["AnotherKey"], Is.EqualTo("AnotherValue"), "Command line args should be available");
   }

   [Test]
   public void should_override_appsettings_with_environment_variables()
   {
      var appSettingsPath = Path.Combine(_testDirectory, "appsettings.json");
      var appSettingsDevelopmentPath = Path.Combine(_testDirectory, "appsettings.Development.json");

      File.WriteAllText(appSettingsPath, @"{""TestKey"": ""BaseValue""}");
      File.WriteAllText(appSettingsDevelopmentPath, @"{""TestKey"": ""DevelopmentValue""}");

      Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");

      var prefix = GetApplicationNamePrefix();
      var envVarName = $"{prefix}TestKey";
      Environment.SetEnvironmentVariable(envVarName, "EnvVarValue");

      using var host = new HostBuilder()
         .UseDefaultBuilder([])
         .Build();

      var configuration = host.Services.GetRequiredService<IConfiguration>();

      Assert.That(configuration["TestKey"], Is.EqualTo("EnvVarValue"),
         "Environment variables should override appsettings");

      Environment.SetEnvironmentVariable(envVarName, null);
   }

   [Test]
   public void should_override_environment_variables_with_command_line_arguments()
   {
      var prefix = GetApplicationNamePrefix();
      var envVarName = $"{prefix}TestKey";
      Environment.SetEnvironmentVariable(envVarName, "EnvVarValue");

      var args = new[] { "--TestKey=CommandLineValue" };

      using var host = new HostBuilder()
         .UseDefaultBuilder(args)
         .Build();

      var configuration = host.Services.GetRequiredService<IConfiguration>();

      Assert.That(configuration["TestKey"], Is.EqualTo("CommandLineValue"),
         "Command line args should override environment variables");

      Environment.SetEnvironmentVariable(envVarName, null);
   }

   [Test]
   public void should_validate_scopes_in_development()
   {
      Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");

      using var host = new HostBuilder()
         .UseDefaultBuilder([])
         .ConfigureServices((_, services) =>
         {
            services.AddScoped<TestScopedService>();
         })
         .Build();

      Assert.Throws<InvalidOperationException>(() =>
      {
         host.Services.GetRequiredService<TestScopedService>();
      });
   }

   [Test]
   public void should_not_validate_scopes_in_production()
   {
      Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");

      using var host = new HostBuilder()
         .UseDefaultBuilder([])
         .ConfigureServices((_, services) =>
         {
            services.AddScoped<TestScopedService>();
         })
         .Build();

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
