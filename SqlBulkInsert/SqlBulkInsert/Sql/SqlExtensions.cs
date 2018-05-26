// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBulkInsert.Sql
{
    internal static class SqlExtensions
    {
        /// <summary>
        /// Get collections from SQL reader.  SQL reader support reading multiple data sets.
        /// This capability allows a single call to a stored procedure to return different data sets
        /// as required, improving performance and providing single snapshot view of related data.
        /// 
        /// This extension is called for every dataset returned by a single call to a stored procedures.
        /// </summary>
        /// <typeparam name="T">type of collection</typeparam>
        /// <param name="reader">SQL reader</param>
        /// <param name="factory">type constructor factory</param>
        /// <returns>List of types returned by SQL.  List can be 0 in length if no data was returned for dataset.</returns>
        public static IList<T> GetCollection<T>(this SqlDataReader reader, Func<SqlDataReader, T> factory)
        {
            var list = new List<T>();

            while (reader.Read())
            {
                list.Add(factory(reader));
            }

            reader.NextResult();

            return list;
        }
    }
}
