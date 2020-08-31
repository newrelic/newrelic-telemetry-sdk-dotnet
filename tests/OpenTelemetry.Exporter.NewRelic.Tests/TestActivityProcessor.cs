using System;
using System.Diagnostics;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Exporter.NewRelic.Tests
{
    internal class TestActivityProcessor : ActivityProcessor
    {
        public Action<Activity> StartAction;
        public Action<Activity> EndAction;

        public TestActivityProcessor()
        {
        }

        public TestActivityProcessor(Action<Activity> onStart, Action<Activity> onEnd)
        {
            this.StartAction = onStart;
            this.EndAction = onEnd;
        }

        public bool ShutdownCalled { get; private set; } = false;

        public bool ForceFlushCalled { get; private set; } = false;

        public bool DisposedCalled { get; private set; } = false;

        public override void OnStart(Activity span)
        {
            this.StartAction?.Invoke(span);
        }

        public override void OnEnd(Activity span)
        {
            this.EndAction?.Invoke(span);
        }

        protected override bool OnForceFlush(int timeoutMilliseconds)
        {
            this.ForceFlushCalled = true;
            return true;
        }

        protected override void OnShutdown(int timeoutMilliseconds)
        {
            this.ShutdownCalled = true;
        }

        protected override void Dispose(bool disposing)
        {
            this.DisposedCalled = true;
        }
    }
}
