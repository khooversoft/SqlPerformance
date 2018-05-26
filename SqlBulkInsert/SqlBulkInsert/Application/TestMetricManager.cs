// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBulkInsert
{
    internal class TestMetricManager : ITestMetricManager
    {
        private readonly List<TestMetric> _metricList = new List<TestMetric>();
        private readonly object _lock = new object();
        private readonly IOptions _options;
        private readonly IConfiguration _configuration;

        public TestMetricManager(IOptions options, IConfiguration configuration)
        {
            _options = options;
            _configuration = configuration;
        }

        public void Add(TestMetric metric)
        {
            if (metric == null) { throw new ArgumentException(nameof(metric)); }

            lock (_lock)
            {
                _metricList.Add(metric);
            }
        }

        public IEnumerator<TestMetric> GetEnumerator()
        {
            lock (_lock)
            {
                return _metricList.ToList().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
