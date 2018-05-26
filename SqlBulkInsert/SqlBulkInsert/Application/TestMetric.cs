// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBulkInsert
{
    internal class TestMetric
    {
        public TestMetric(string name, int count, TimeSpan timePeriod, int clientCount, int batchSize)
        {
            Name = name;
            Count = count;
            TimePeriod = timePeriod;
            ClientCount = clientCount;
            BatchSize = batchSize;
            Tps = Count / TimePeriod.TotalSeconds;
        }

        public string Name { get; }

        public int Count { get; }

        public TimeSpan TimePeriod { get; }

        public int ClientCount { get; }

        public int BatchSize { get; }

        public double Tps { get; }

        public override string ToString() => $"{Name,-25}, Clients={ClientCount,3:#0}, BatchSize={BatchSize,5:#0}, Count={Count,13:#,##0}, Period={TimePeriod}, Tps={Tps,12:#,##0.00}";
    }
}
