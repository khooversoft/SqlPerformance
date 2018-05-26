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
    /// Row with object variable (DbType.Invariant)
    /// </summary>
    internal class Row
    {
        public Row(int index)
        {
            object variable = null;

            switch (index % 3)
            {
                case 0:
                    variable = 3;
                    break;

                case 1:
                    variable = $"Test {Guid.NewGuid().ToString()}";
                    break;

                case 2:
                    variable = 5.3f;
                    break;
            }

            Id = index;
            Variable = variable;
            Description = $"Row item {index}";
        }

        public int Id { get; set; }

        public object Variable { get; set; }

        public string Description { get; set; }
    }
}
