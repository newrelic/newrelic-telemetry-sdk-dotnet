// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using OpenTelemetry;

namespace NewRelic.OpenTelemetry.Tests
{
    internal class TestActivityProcessor : BaseProcessor<Activity>
    {
        public Action<Activity>? StartAction;
        public Action<Activity>? EndAction;

        public TestActivityProcessor()
        {
        }

        public TestActivityProcessor(Action<Activity> onStart, Action<Activity> onEnd)
        {
            StartAction = onStart;
            EndAction = onEnd;
        }

        public bool ShutdownCalled { get; private set; } = false;

        public bool ForceFlushCalled { get; private set; } = false;

        public bool DisposedCalled { get; private set; } = false;

        public override void OnStart(Activity span)
        {
            StartAction?.Invoke(span);
        }

        public override void OnEnd(Activity span)
        {
            EndAction?.Invoke(span);
        }

        protected override bool OnForceFlush(int timeoutMilliseconds)
        {
            ForceFlushCalled = true;
            return true;
        }

        protected override bool OnShutdown(int timeoutMilliseconds)
        {
            ShutdownCalled = true;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            DisposedCalled = true;
        }
    }
}
