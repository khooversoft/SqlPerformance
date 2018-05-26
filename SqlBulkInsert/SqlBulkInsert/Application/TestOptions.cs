// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBulkInsert
{
    internal class TestOptions : IOptions
    {
        public TestOptions(Operation operation, int batchSize, int clientCount, TimeSpan? timeLimit)
        {
            Operation = operation;
            BatchSize = batchSize;
            ClientCount = clientCount;
            TimeLimit = timeLimit;
        }

        public bool Help { get; } = false;

        public Operation Operation { get; }

        public int BatchSize { get; }

        public TimeSpan SampleRate { get; } = TimeSpan.FromSeconds(5);

        public int ClientCount { get; }

        public TimeSpan? TimeLimit { get; }

        public int MaxClientCount => throw new NotImplementedException();

        public string LoggingFolder => throw new NotImplementedException();

        public void DisplayOptions() => throw new NotImplementedException();
    }
}
