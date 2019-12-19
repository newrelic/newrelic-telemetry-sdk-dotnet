using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using Xunit.Abstractions;

namespace IntegrationTests.Fixtures
{
    public abstract class BaseApplication
    {
        protected Process _applicationProcess;

        public string ApplicationName { get; }

        public string[] ServiceNames { get; }

        public Dictionary<string, string> UserProvidedEnvironmentVariables { get; set; }

        public ITestOutputHelper TestLogger { get; set; }

        protected BaseApplication(string applicationName, string[] serviceNames)
        {
            ApplicationName = applicationName;
            ServiceNames = serviceNames;
        }

        public static string IntegrationTestsStartingDirectoryPath { get; } = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\");

        public abstract void Run();

        public virtual List<string> NugetSources { get; } = new List<string>
        {
            "https://api.nuget.org/v3/index.json"
        };

        public Process InvokeAnExecutable(string executablePath, string arguments, string workingDirectory, bool waitForExit, Dictionary<string, string> environmentVariables)
        {
            var startInfo = new ProcessStartInfo
            {
                Arguments = arguments,
                FileName = executablePath,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            foreach(var env in environmentVariables)
            {
                startInfo.EnvironmentVariables.Add(env.Key, env.Value);
            }


            Process process = Process.Start(startInfo);

            if (process == null)
            {
                throw new Exception($@"[{DateTime.Now}] {executablePath} process failed to start.");
            }

            LogProcessOutput(process.StandardOutput);
            LogProcessOutput(process.StandardError);

            if (waitForExit)
            {
                process.WaitForExit();
            }

            

            if (process.HasExited && process.ExitCode != 0)
            {
                throw new Exception("App server shutdown unexpectedly.");
            }

            return process;

        }

        private async void LogProcessOutput(TextReader reader)
        {
            string line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                TestLogger?.WriteLine($@"[{DateTime.Now}] {line}");
            }
        }

        public abstract void InstallNugetPackages();
        public abstract void Build();
        public abstract void StopApplication();
    }
}
