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
    /// Test action using SqlBulkCopy and custom IDataReader over List collection
    /// </summary>
    internal class SqlBulkCopyAction : ActionClientBase, IAction
    {
        public SqlBulkCopyAction(IOptions options, IConfiguration configuration, ILogging logging, ITestMetricManager testMetricManager)
            : base(options, configuration, logging, testMetricManager)
        {
            ClientProcess = (monitor, clientNumber, token) => InternalClientProcess(monitor, clientNumber, token);
        }

        private async Task InternalClientProcess(MonitorReport report, int clientNumber, CancellationToken token)
        {
            string agentName = $"Client_{clientNumber}";

            var itemList = new List<Row>();

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
                        itemList.Add(new Row(id));
                    }

                    using (var conn = new SqlConnection(_configuration.SqlConnectionString))
                    {
                        conn.Open();

                        using (var bulkCopy = new SqlBulkCopy(_configuration.SqlConnectionString))
                        {
                            bulkCopy.DestinationTableName = "[App].[Import]";
                            var dataReader = new DataReader(itemList);
                            await bulkCopy.WriteToServerAsync(dataReader, token);
                        }
                    }

                    monitor.IncrementBatch();
                    monitor.IncrementNew(itemList.Count);

                    itemList.Clear();
                }
            }
        }

        public override async Task<int> GetCount()
        {
            return await new SqlExec(_configuration)
                .SetCommand("select count(*) from app.Import", CommandType.Text)
                .ExecuteScalarAsync<int>();
        }

        internal class DataReader : IDataReader
        {
            private static readonly List<SqlColumnDefintion<Row>> _definations;
            private int _index = -1;

            static DataReader()
            {
                _definations = new List<SqlColumnDefintion<Row>>
                {
                    new SqlColumnDefintion<Row>(nameof(Row.Id), SqlDbType.Int, x => x.Id),
                    new SqlColumnDefintion<Row>(nameof(Row.Variable), SqlDbType.Variant, x => x.Variable),
                    new SqlColumnDefintion<Row>(nameof(Row.Description), SqlDbType.NVarChar, x => x.Description, 50),
                };
            }

            public DataReader(List<Row> rows)
            {
                Rows = rows;
            }

            public List<Row> Rows { get; }

            public bool IsClosed => false;
            public int FieldCount => _definations.Count;

            public object this[int i] => throw new NotImplementedException();
            public object this[string name] => throw new NotImplementedException();
            public int Depth => throw new NotImplementedException();
            public int RecordsAffected => throw new NotImplementedException();

            public void Close()
            {
            }

            public void Dispose()
            {
            }

            public bool Read()
            {
                _index++;
                return _index < Rows.Count ? true : false;
            }

            public string GetName(int i)
            {
                return i < _definations.Count ? _definations[i].ColumnName : string.Empty;
            }

            public int GetOrdinal(string name)
            {
                return _definations
                    .Select((x, i) => new { Def = x, Index = i })
                    .Where(x => x.Def.ColumnName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault()?.Index ?? -1;
            }

            public object GetValue(int i)
            {
                return i < _definations.Count ? _definations[i].GetValue(Rows[_index]) : null;
            }

            public bool GetBoolean(int i) { throw new NotImplementedException(); }
            public byte GetByte(int i) { throw new NotImplementedException(); }
            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) { throw new NotImplementedException(); }
            public char GetChar(int i) { throw new NotImplementedException(); }
            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) { throw new NotImplementedException(); }
            public IDataReader GetData(int i) { throw new NotImplementedException(); }
            public string GetDataTypeName(int i) { throw new NotImplementedException(); }
            public DateTime GetDateTime(int i) { throw new NotImplementedException(); }
            public decimal GetDecimal(int i) { throw new NotImplementedException(); }
            public double GetDouble(int i) { throw new NotImplementedException(); }
            public Type GetFieldType(int i) { throw new NotImplementedException(); }
            public float GetFloat(int i) { throw new NotImplementedException(); }
            public Guid GetGuid(int i) { throw new NotImplementedException(); }
            public short GetInt16(int i) { throw new NotImplementedException(); }
            public int GetInt32(int i) { throw new NotImplementedException(); }
            public long GetInt64(int i) { throw new NotImplementedException(); }
            public DataTable GetSchemaTable() { throw new NotImplementedException(); }
            public string GetString(int i) { throw new NotImplementedException(); }
            public int GetValues(object[] values) { throw new NotImplementedException(); }
            public bool IsDBNull(int i) { throw new NotImplementedException(); }
            public bool NextResult() { throw new NotImplementedException(); }
        }
    }
}
