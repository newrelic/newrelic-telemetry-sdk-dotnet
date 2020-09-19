// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace NewRelic.Telemetry.Tests
{
    public class CustomLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new ConcurrentDictionary<string, ILogger>();

        private ConcurrentDictionary<string, List<string>>? _logOutput;

        public ConcurrentDictionary<string, List<string>> LogOutput
        {
            get
            {
                if (_logOutput == null)
                {
                    _logOutput = new ConcurrentDictionary<string, List<string>>();
                }

                return _logOutput;
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, new CustomLogger(this, categoryName));
        }

        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                _disposedValue = true;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access", Justification = "This is more readable the end.")]
        void IDisposable.Dispose()
        {
            Dispose(true);
        }
    }
 }
