// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTelemetry.Exporter.NewRelic.Tests
{
    internal class TestHttpServer
    {
        private static readonly Random GlobalRandom = new Random();

        public static IDisposable RunServer(Action<HttpListenerContext> action, out string host, out int port)
        {
            host = "localhost";
            port = 0;
            RunningServer server = null;

            var retryCount = 5;
            while (retryCount > 0)
            {
                try
                {
                    port = GlobalRandom.Next(2000, 5000);
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
            private readonly Task httpListenerTask;
            private readonly HttpListener listener;
            private readonly AutoResetEvent initialized = new AutoResetEvent(false);

            public RunningServer(Action<HttpListenerContext> action, string host, int port)
            {
                this.listener = new HttpListener();

                this.listener.Prefixes.Add($"http://{host}:{port}/");
                this.listener.Start();

                this.httpListenerTask = new Task(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            var ctxTask = this.listener.GetContextAsync();

                            this.initialized.Set();

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
                this.httpListenerTask.Start();
                this.initialized.WaitOne();
            }

            public void Dispose()
            {
                try
                {
                    this.listener?.Stop();
                }
                catch (ObjectDisposedException)
                {
                    // swallow this exception just in case
                }
            }
        }
    }
}
