// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBulkInsert
{
    internal class Logging : IDisposable, ILogging
    {
        private readonly IOptions _options;
        private StreamWriter _file;
        private long _counter;
        private DateTime _lastFlush;

        public Logging(IOptions options)
        {
            _options = options;

            if (!string.IsNullOrWhiteSpace(_options.LoggingFolder))
            {
                Directory.CreateDirectory(_options.LoggingFolder);
                _file = new StreamWriter(Path.Combine(_options.LoggingFolder, $"Log_{Guid.NewGuid().ToString()}.txt"));
            }
        }

        public void Dispose()
        {
            var file = Interlocked.Exchange(ref _file, null);
            file?.Close();
        }

        public void Error(Func<string> log)
        {
            WriteLogLine("Error", log());
        }

        public void Log(Func<string> log)
        {
            if (log == null)
            {
                Console.WriteLine();
                return;
            }

            WriteLogLine("Detail", log());
        }

        private void WriteLogLine(string logType, string message)
        {
            Console.WriteLine(message);

            if (_file == null)
            {
                return;
            }

            message = message ?? "***";
            long counter = Interlocked.Increment(ref _counter);

            _file.WriteLine($"{counter} : {DateTime.Now.ToString("o")} : {Thread.CurrentThread.ManagedThreadId} : {logType} : {message}");

            if (DateTime.Now > _lastFlush.AddSeconds(10))
            {
                _file.Flush();
                _lastFlush = DateTime.Now;
            }
        }
    }
}
