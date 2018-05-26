// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBulkInsert.Sql
{
    /// <summary>
    /// SQL Execute, primary abstraction for ADO.NET with strong contracts
    /// 
    /// If a deadlock is detected, the sql command will be retried n times with a random back off delay between 10 and 1000 ms.
    /// </summary>
    internal class SqlExec
    {
        private static readonly Random _random = new Random();
        private const int _retryCount = 5;
        private const int _deadLockNumber = 1205;
        private const string _deadLockMessage = "Deadlock retry failed";

        public SqlExec(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// SQL configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// SQL command
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        /// SQL command type
        /// </summary>
        public CommandType CommandType { get; private set; } = CommandType.StoredProcedure;

        /// <summary>
        /// Parameters
        /// </summary>
        public IList<ISqlParameter> Parameters { get; } = new List<ISqlParameter>();

        /// <summary>
        /// Set SQL command
        /// </summary>
        /// <param name="command">SQL command to use</param>
        /// <returns>this</returns>
        public SqlExec SetCommand(string command, CommandType commandType = CommandType.StoredProcedure)
        {
            Command = command;
            CommandType = commandType;

            return this;
        }

        /// <summary>
        /// Set parameter
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="value">value</param>
        /// <param name="addValueIfNull">Add value if null</param>
        /// <returns>this</returns>
        public SqlExec AddParameter<T>(string name, T value, bool addValueIfNull = false)
        {
            if (!addValueIfNull && value == null)
            {
                return this;
            }

            if (typeof(T).IsEnum)
            {
                Parameters.Add(new SqlSimpleParameter(name, value.ToString()));
                return this;
            }

            Parameters.Add(new SqlSimpleParameter(name, value));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public SqlExec AddParameter(ISqlParameter parameter)
        {
            Parameters.Add(parameter);
            return this;
        }

        /// <summary>
        /// Execute None SQL query (no response)
        /// </summary>
        /// <returns>task</returns>
        public async Task ExecuteNonQuery()
        {
            SqlException saveEx = null;

            using (var conn = new SqlConnection(Configuration.SqlConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = Command;
                cmd.CommandType = CommandType;
                cmd.Parameters.AddRange(Parameters.Select(x => x.ToSqlParameter()).ToArray());

                conn.Open();

                for (int retry = 0; retry < _retryCount; retry++)
                {
                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                        return;
                    }
                    catch (SqlException sqlEx)
                    {
                        if (sqlEx.Number == _deadLockNumber)
                        {
                            saveEx = sqlEx;
                            Thread.Sleep(TimeSpan.FromMilliseconds(_random.Next(10, 1000)));
                            continue;
                        }

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Execute None SQL query (no response)
        /// </summary>
        /// <typeparam name="T">value type</typeparam>
        /// <returns>task</returns>
        public async Task<T> ExecuteScalarAsync<T>() where T : struct
        {
            SqlException saveEx = null;

            using (var conn = new SqlConnection(Configuration.SqlConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = Command;
                cmd.CommandType = CommandType;
                cmd.Parameters.AddRange(Parameters.Select(x => x.ToSqlParameter()).ToArray());

                conn.Open();

                for (int retry = 0; retry < _retryCount; retry++)
                {
                    try
                    {
                        object obj = await cmd.ExecuteScalarAsync();
                        return (T)Convert.ChangeType(obj, typeof(T));
                    }
                    catch (SqlException sqlEx)
                    {
                        if (sqlEx.Number == _deadLockNumber)
                        {
                            saveEx = sqlEx;
                            Thread.Sleep(TimeSpan.FromMilliseconds(_random.Next(10, 1000)));
                            continue;
                        }

                        throw;
                    }
                }
            }

            throw new InvalidOperationException(_deadLockMessage, saveEx);
        }

        /// <summary>
        /// Execute SQL and return data set
        /// </summary>
        /// <typeparam name="T">type to return</typeparam>
        /// <param name="factory">type factor</param>
        /// <returns>list of types</returns>
        public async Task<IList<T>> Execute<T>(Func<SqlDataReader, T> factory)
        {
            return await ExecuteBatch((r) => r.GetCollection(factory));
        }

        /// <summary>
        /// Execute SQL and return results in DataTable
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>data table</returns>
        public async Task<DataTable> ExecuteDataTable()
        {
            return await ExecuteBatch<DataTable>((r) =>
            {
                var dt = new DataTable();
                dt.Load(r);
                return dt;
            });
        }

        /// <summary>
        /// Execute SQL and batch object
        /// </summary>
        /// <typeparam name="T">type to return</typeparam>
        /// <param name="context">execution context</param>
        /// <param name="factory">batch type factor</param>
        /// <returns>list of types</returns>
        public async Task<T> ExecuteBatch<T>(Func<SqlDataReader, T> factory)
        {
            SqlException saveEx = null;

            using (var conn = new SqlConnection(Configuration.SqlConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = Command;
                cmd.CommandType = CommandType;
                cmd.Parameters.AddRange(Parameters.Select(x => x.ToSqlParameter()).ToArray());

                conn.Open();

                for (int retry = 0; retry < _retryCount; retry++)
                {
                    try
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            return factory(reader);
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        if (sqlEx.Number == _deadLockNumber)
                        {
                            saveEx = sqlEx;
                            await Task.Delay(TimeSpan.FromMilliseconds(_random.Next(10, 1000)));
                            continue;
                        }

                        throw;
                    }
                }
            }

            throw new InvalidOperationException(_deadLockMessage, saveEx);
        }
    }
}
