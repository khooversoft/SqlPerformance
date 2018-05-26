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
    /// Test action using SQL stored procedures with User Defined Table type
    /// </summary>
    internal class StoredProcedureBulkCopy : ActionClientBase, IAction
    {
        public StoredProcedureBulkCopy(IOptions options, IConfiguration configuration, ILogging logging, ITestMetricManager testMetricManager)
            : base(options, configuration, logging, testMetricManager)
        {
            ClientProcess = (monitor, clientNumber, token) => InternalClientProcess(monitor, clientNumber, token);
        }

        private async Task InternalClientProcess(MonitorReport report, int clientNumber, CancellationToken token)
        {
            string agentName = $"Client_{clientNumber}";
            var table = new SqlTableParameter<Row>("items", "[dbo].[ImportList]");

            table.ColumnDefinitions.Add(new SqlColumnDefintion<Row>(nameof(Row.Id), SqlDbType.Int, x => x.Id));
            table.ColumnDefinitions.Add(new SqlColumnDefintion<Row>(nameof(Row.Variable), SqlDbType.Variant, x => x.Variable));
            table.ColumnDefinitions.Add(new SqlColumnDefintion<Row>(nameof(Row.Description), SqlDbType.NVarChar, x => x.Description, 50));

            using (var monitor = new MonitorRate(report, agentName))
            {
                while (!token.IsCancellationRequested)
                {
                    foreach (int index in Enumerable.Range(0, _options.BatchSize))
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        int id = Interlocked.Increment(ref _idCounter);
                        table.Items.Add(new Row(id));
                    }

                    await new SqlExec(_configuration)
                        .SetCommand("[App].[InsertIntoImport]", CommandType.StoredProcedure)
                        .AddParameter(table)
                        .ExecuteNonQuery();

                    monitor.IncrementBatch();
                    monitor.IncrementNew(table.Items.Count);

                    table.Items.Clear();
                }
            }
        }

        public override async Task<int> GetCount()
        {
            return await new SqlExec(_configuration)
                .SetCommand("select count(*) from app.Import", CommandType.Text)
                .ExecuteScalarAsync<int>();
        }
    }
}
