// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using SqlBulkInsert.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBulkInsert
{
    /// <summary>
    /// Test action for Bulk copy data using DataTable and SqlBulkCopy
    /// </summary>
    internal class DataTableAction : ActionClientBase, IAction
    {
        public DataTableAction(IOptions options, IConfiguration configuration, ILogging logging, ITestMetricManager testMetricManager)
            : base(options, configuration, logging, testMetricManager)
        {
            ClientProcess = (monitor, clientNumber, token) => InternalClientProcess(monitor, clientNumber, token);
        }

        private async Task InternalClientProcess(MonitorReport report, int clientNumber, CancellationToken token)
        {
            string agentName = $"Client_{clientNumber}";

            using (var monitor = new MonitorRate(report, agentName))
            {
                while (!token.IsCancellationRequested)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Id", typeof(int));
                    dt.Columns.Add("Variable", typeof(int));
                    dt.Columns.Add("Description", typeof(string));

                    foreach (int index in Enumerable.Range(0, _options.BatchSize))
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        int id = Interlocked.Increment(ref _idCounter);
                        var row = new Row2(id);
                        dt.Rows.Add(row.Id, row.Variable, row.Description);
                    }

                    using (var conn = new SqlConnection(_configuration.SqlConnectionString))
                    {
                        conn.Open();

                        using (var bulkCopy = new SqlBulkCopy(_configuration.SqlConnectionString))
                        {
                            bulkCopy.DestinationTableName = "[App].[Import2]";
                            await bulkCopy.WriteToServerAsync(dt, token);
                        }
                    }

                    monitor.IncrementBatch();
                    monitor.IncrementNew(dt.Rows.Count);
                }
            }
        }

        public override async Task<int> GetCount()
        {
            return await new SqlExec(_configuration)
                .SetCommand("select count(*) from app.Import2", CommandType.Text)
                .ExecuteScalarAsync<int>();
        }
    }
}
