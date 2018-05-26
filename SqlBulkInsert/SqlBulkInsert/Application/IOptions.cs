// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBulkInsert
{
    internal interface IOptions
    {
        bool Help { get; }

        Operation Operation { get; }

        int BatchSize { get; }

        TimeSpan SampleRate { get; }

        int ClientCount { get; }

        TimeSpan? TimeLimit { get; }

        int MaxClientCount { get; }

        string LoggingFolder { get; }

        void DisplayOptions();
    }
}
