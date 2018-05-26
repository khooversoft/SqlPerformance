// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBulkInsert
{
    /// <summary>
    /// Row object used for non-variant test
    /// </summary>
    internal class Row2
    {
        private static readonly Random _random = new Random();

        public Row2(int index)
        {
            Id = index;
            Variable = _random.Next();
            Description = $"Row item {index}";
        }

        public int Id { get; set; }

        public int Variable { get; set; }

        public string Description { get; set; }
    }
}
