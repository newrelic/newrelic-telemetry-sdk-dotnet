using System;
using System.Threading;
using Xunit.Abstractions;

namespace IntegrationTests.Fixtures
{
    public class BaseFixture : IDisposable
    {
        public BaseApplication Application { get; }
        public ITestOutputHelper? TestLogger { get; set; }

        private bool _initialized;
        public bool Initialized => _initialized;

        public BaseFixture(BaseApplication application)
        {
            Application = application;
        }

        public Action? Exercise { get; set; }

        public void Initialize()
        {
            Application.TestLogger = TestLogger;

            Application.Build();

            Application.Run();

            //Give the test app some time to start. 
            Thread.Sleep(5000);

            TestLogger?.WriteLine($@"[{DateTime.Now}] ... Testing");

            if(Exercise == null)
            {
                throw new Exception("Exercise delegate is null");
            }

            Exercise.Invoke();

            TestLogger?.WriteLine($@"[{DateTime.Now}] ... Testing done");

            _initialized = true;
        }

        public void Dispose()
        {
            Application.StopApplication();

        }
    }
}
