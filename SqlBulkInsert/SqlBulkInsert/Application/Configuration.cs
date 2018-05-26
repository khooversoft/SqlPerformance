// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBulkInsert
{
    internal class Configuration : IConfiguration
    {
        public Configuration()
        {
            SqlConnectionString = new SqlConnectionStringBuilder("Data Source=.;Integrated Security=True;") { InitialCatalog = "SqlBulkInsertTest" }.ToString();
        }

        public string SqlConnectionString { get; }
    }
}
