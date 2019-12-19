using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace IntegrationTests.Fixtures
{
    public class OpenTelemetryUsageApplication : BaseApplication
    {

        private const string TestPackageName = "OpenTelemetry.Exporter.NewRelic";

        private readonly string TestPackagesDirectoryPath;

        public string TestSampleApplicationDirectory { get; }

        public string SolutionConfiguration { get; }

        public string ApplicationOutputPath { get; private set; }

        public OpenTelemetryUsageApplication(string applicationName, string[] serviceNames) : base(applicationName, serviceNames)
        {
            TestPackagesDirectoryPath = Path.GetFullPath(Path.Combine(IntegrationTestsStartingDirectoryPath, $@"TestNugetPackages"));
 
            NugetSources.Add(TestPackagesDirectoryPath);
 
            TestSampleApplicationDirectory = Path.GetFullPath(Path.Combine(IntegrationTestsStartingDirectoryPath, $@"Applications\{ApplicationName}"));

            SolutionConfiguration = "Release";
#if DEBUG
            SolutionConfiguration = "Debug";
#endif
            ApplicationOutputPath = Path.Combine(TestSampleApplicationDirectory, $@"bin\{SolutionConfiguration}\netcoreapp3.0");
        }

        public override void InstallNugetPackages()
        {
            var version = GetNugetPackageVersion(TestPackagesDirectoryPath);

            TestLogger?.WriteLine($@"[{DateTime.Now}] Installing {TestPackageName} version {version} .");

            UpdatePackageReference(TestPackageName, version, NugetSources);

            RestoreNuGetPackage(NugetSources);

            TestLogger?.WriteLine($@"[{DateTime.Now}] {TestPackageName} version {version} installed.");
        }

        public override void Build()
        {
            try
            {
                var workingDirectory = TestSampleApplicationDirectory;

                var arguments = $@"build";

                InvokeAnExecutable("dotnet.exe", arguments, workingDirectory, true, UserProvidedEnvironmentVariables);

            }
            catch (Exception ex)
            {
                TestLogger?.WriteLine($@"[{DateTime.Now}] Failed to build the {ApplicationName} application. Exception: {ex.ToString()}");
            }
        }

        public override void Run()
        {
            try
            {
                var workingDirectory = TestSampleApplicationDirectory;

                var arguments = $@"run";

                _applicationProcess = InvokeAnExecutable("dotnet.exe", arguments, workingDirectory, false, UserProvidedEnvironmentVariables);

            }
            catch(Exception ex)
            {
                TestLogger?.WriteLine($@"[{DateTime.Now}] Failed to run the {ApplicationName} application. Exception: {ex.ToString()}");
            }
        }

        public override void StopApplication()
        {
            _applicationProcess?.Kill();
        }

        private string GetNugetPackageVersion(string nugetSource)
        {
            var package = Directory.GetFiles(nugetSource).FirstOrDefault();
            if (package != null)
            {
                var parts = package.Split('.');

                return $@"{parts[^4]}.{parts[^3]}.{parts[^2]}";
            }

            return string.Empty;
        }

        private void RestoreNuGetPackage(List<string> sources)
        {

            TestLogger?.WriteLine($@"[{DateTime.Now}] Restoring NuGet packages.");

            var sourceArgument = string.Join(";", sources);

            var arguments = $@"restore --source ""{sourceArgument}"" --no-cache";

            try
            {
                InvokeAnExecutable("dotnet.exe", arguments, TestSampleApplicationDirectory, true, UserProvidedEnvironmentVariables);
            }
            catch (Exception ex)
            {
                TestLogger?.WriteLine($@"[{DateTime.Now}] Failed to restore nuget packages. Exception: {ex.ToString()}");
                throw new Exception($@"There were errors while restoring nuget packages");
            }

            TestLogger?.WriteLine($@"[{DateTime.Now}] Nuget packages restored.");
        }

        private void UpdatePackageReference(string packageName, string version, List<string> sources)
        {
            try
            {
                var workingDirectory = TestSampleApplicationDirectory;
                var sourceArgument = string.Join(";", sources);

                var arguments = $@"add package {packageName} -v {version} -s {sourceArgument}";

                InvokeAnExecutable("dotnet.exe", arguments, workingDirectory, true, UserProvidedEnvironmentVariables);
            }
            catch (Exception ex)
            {
                TestLogger?.WriteLine($@"[{DateTime.Now}] Failed to add {packageName} version {version} nuget package. Exception: {ex.ToString()}");
            }
        }
    }
}
