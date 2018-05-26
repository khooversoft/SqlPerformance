// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using SqlBulkInsert.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBulkInsert
{
    /// <summary>
    /// Clear test tables
    /// </summary>
    internal class ResetAction : IAction
    {
        private readonly IOptions _options;
        private readonly IConfiguration _configuration;

        public ResetAction(IOptions options, IConfiguration configuration)
        {
            _options = options;
            _configuration = configuration;
        }

        public async Task Process(CancellationToken token)
        {
            await new SqlExec(_configuration)
                .SetCommand("[App].[Reset]", CommandType.StoredProcedure)
                .ExecuteNonQuery();

            Console.WriteLine("Reset executed to clear the import table.");
        }
    }
}
