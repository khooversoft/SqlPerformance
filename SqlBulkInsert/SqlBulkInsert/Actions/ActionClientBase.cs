// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBulkInsert
{
    /// <summary>
    /// Provides support for running multiple clients in parallel and collecting metrics
    /// </summary>
    internal abstract class ActionClientBase
    {
        protected readonly IOptions _options;
        protected readonly IConfiguration _configuration;
        protected readonly ILogging _logging;
        protected readonly ITestMetricManager _testMetricManager;
        protected int _idCounter = 0;

        public ActionClientBase(IOptions options, IConfiguration configuration, ILogging logging, ITestMetricManager testMetricManager)
        {
            _options = options;
            _configuration = configuration;
            _logging = logging;
            _testMetricManager = testMetricManager;
        }

        public Func<MonitorReport, int, CancellationToken, Task> ClientProcess { get; protected set; }

        public virtual Task<int> GetCount() { return Task.FromResult(0); }

        public virtual async Task Process(CancellationToken outterToken)
        {
            _logging.Log(() => $"Starting {GetType().Name} test");
            Interlocked.Exchange(ref _idCounter, 0);

            if (ClientProcess == null)
            {
                throw new ArgumentException($"{nameof(ClientProcess)} is null");
            }

            DateTime start = DateTime.Now;
            try
            {
                using (var report = new MonitorReport(GetType().Name, _options, _logging))
                {
                    var tasks = new List<Task>();

                    foreach (var clientNumber in Enumerable.Range(0, _options.ClientCount))
                    {
                        tasks.Add(Task.Run(() => ClientProcess(report, clientNumber, outterToken)));
                    }

                    Task.WaitAll(tasks.ToArray());
                }
            }
            finally
            {
                int count = await GetCount();
                _testMetricManager.Add(new TestMetric(GetType().Name, count, DateTime.Now - start, _options.ClientCount, _options.BatchSize));
            }
        }
    }
}
