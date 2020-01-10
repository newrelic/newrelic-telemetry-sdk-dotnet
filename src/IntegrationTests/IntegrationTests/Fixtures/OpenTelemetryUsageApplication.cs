using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IntegrationTests.Fixtures
{
    public class OpenTelemetryUsageApplication : BaseApplication
    {
        private const string TestPackageName = "OpenTelemetry.Exporter.NewRelic";

        private readonly string TestPackageVersion;

        public string TestSampleApplicationDirectory { get; }

        public string SolutionConfiguration { get; }

        public string ApplicationOutputPath { get; private set; }

        public OpenTelemetryUsageApplication(string applicationName, string[] serviceNames) : base(applicationName, serviceNames)
        {
            SolutionConfiguration = "Release";
#if DEBUG
            SolutionConfiguration = "Debug";
#endif
            var localNugetPakageSrcPath = @$"{Path.GetFullPath(SrcDirectoryPath)}\LocalNugetPackageSource";;

            NugetSources.Insert(0, localNugetPakageSrcPath);

            TestPackageVersion = GetNugetPackageVersion(localNugetPakageSrcPath, TestPackageName);
            TestSampleApplicationDirectory = Path.GetFullPath(Path.Combine(IntegrationTestsStartingDirectoryPath, $@"Applications\{ApplicationName}"));

            ApplicationOutputPath = Path.Combine(TestSampleApplicationDirectory, $@"bin\{SolutionConfiguration}\netcoreapp3.0");
        }

        public override void Build()
        {
            try
            {
                InstallTestNugetPackages();

                var workingDirectory = TestSampleApplicationDirectory;

                var arguments = $@"build --configuration {SolutionConfiguration}";

                InvokeAnExecutable("dotnet.exe", arguments, workingDirectory, true);

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

                var arguments = $@"""{TestSampleApplicationDirectory}\bin\{SolutionConfiguration}\netcoreapp3.0\{ApplicationName}.dll"" --no-build";

                _applicationProcess = InvokeAnExecutable("dotnet.exe", arguments, workingDirectory, false);

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

        private void InstallTestNugetPackages()
        {
            TestLogger?.WriteLine($@"[{DateTime.Now}] Installing {TestPackageName} version {TestPackageVersion} .");

            AddPackage(TestPackageName, TestPackageVersion, NugetSources);

            RestoreNuGetPackage(NugetSources);

            TestLogger?.WriteLine($@"[{DateTime.Now}] {TestPackageName} version {TestPackageVersion} installed.");
        }

        private void AddPackage(string packageName, string version, List<string> sources)
        {
            try
            {
                var workingDirectory = TestSampleApplicationDirectory;
                var sourceArgument = string.Join(";", sources);

                var arguments = $@"add package {packageName} -v {version} -s {sourceArgument}";

                InvokeAnExecutable("dotnet.exe", arguments, workingDirectory, true);
            }
            catch (Exception ex)
            {
                TestLogger?.WriteLine($@"[{DateTime.Now}] Failed to add {packageName} version {version} nuget package. Exception: {ex.ToString()}");
            }
        }

        private string GetNugetPackageVersion(string nugetSource, string packageName)
        {
            var package = Directory.GetFiles(nugetSource, packageName + "*").FirstOrDefault();
            package = Path.GetFileName(package);
            if (package != null)
            {
                package = package.Replace(packageName + ".", string.Empty);
                package = package.Replace(".nupkg", string.Empty);
                return package;
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
                InvokeAnExecutable("dotnet.exe", arguments, TestSampleApplicationDirectory, true);
            }
            catch (Exception ex)
            {
                TestLogger?.WriteLine($@"[{DateTime.Now}] Failed to restore nuget packages. Exception: {ex.ToString()}");
                throw new Exception($@"There were errors while restoring nuget packages");
            }

            TestLogger?.WriteLine($@"[{DateTime.Now}] Nuget packages restored.");
        }
    }
}
