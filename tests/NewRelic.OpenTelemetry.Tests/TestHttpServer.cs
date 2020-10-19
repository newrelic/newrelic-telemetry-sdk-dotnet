// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NewRelic.OpenTelemetry.Tests
{
    internal class TestHttpServer
    {
        private static readonly Random _globalRandom = new Random();

        public static IDisposable? RunServer(Action<HttpListenerContext> action, out string host, out int port)
        {
            host = "localhost";
            port = 0;
            RunningServer? server = null;

            var retryCount = 5;
            while (retryCount > 0)
            {
                try
                {
                    port = _globalRandom.Next(2000, 5000);
                    server = new RunningServer(action, host, port);
                    server.Start();
                    break;
                }
                catch (HttpListenerException)
                {
                    retryCount--;
                }
            }

            return server;
        }

        private class RunningServer : IDisposable
        {
            private readonly Task _httpListenerTask;
            private readonly HttpListener _listener;
            private readonly AutoResetEvent _initialized = new AutoResetEvent(false);

            public RunningServer(Action<HttpListenerContext> action, string host, int port)
            {
                _listener = new HttpListener();

                _listener.Prefixes.Add($"http://{host}:{port}/");
                _listener.Start();

                _httpListenerTask = new Task(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            var ctxTask = _listener.GetContextAsync();

                            _initialized.Set();

                            action(await ctxTask.ConfigureAwait(false));
                        }
                        catch (Exception ex)
                        {
                            if (ex is ObjectDisposedException
                                || (ex is HttpListenerException httpEx && httpEx.ErrorCode == 995))
                            {
                                // Listener was closed before we got into GetContextAsync or
                                // Listener was closed while we were in GetContextAsync.
                                break;
                            }

                            throw;
                        }
                    }
                });
            }

            public void Start()
            {
                _httpListenerTask.Start();
                _initialized.WaitOne();
            }

            public void Dispose()
            {
                try
                {
                    _listener?.Stop();
                }
                catch (ObjectDisposedException)
                {
                    // swallow this exception just in case
                }
            }
        }
    }
}
