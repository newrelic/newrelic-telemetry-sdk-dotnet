using System;
using System.IO;

namespace IntegrationTests.Fixtures
{
    public class OpenTelemetryUsageApplication : BaseApplication
    {

        public string TestSampleApplicationDirectory { get; }

        public string SolutionConfiguration { get; }

        public string ApplicationOutputPath { get; private set; }

        public OpenTelemetryUsageApplication(string applicationName, string[] serviceNames) : base(applicationName, serviceNames)
        {
            SolutionConfiguration = "Release";
#if DEBUG
            SolutionConfiguration = "Debug";
#endif
            TestSampleApplicationDirectory = Path.GetFullPath(Path.Combine(IntegrationTestsStartingDirectoryPath, $@"Applications\{ApplicationName}"));

            ApplicationOutputPath = Path.Combine(TestSampleApplicationDirectory, $@"bin\{SolutionConfiguration}\netcoreapp3.0");
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
    }
}
