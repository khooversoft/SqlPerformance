// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqlBulkInsert
{
    internal class Options : IOptions
    {
        private readonly Dictionary<string, Action<string>> _commands;
        private readonly Dictionary<string, Action> _switches;
        private bool? _help;

        private Options()
        {
            _commands = new Dictionary<string, Action<string>>
            {
                ["-BatchSize"] = x => BatchSize = Math.Max(10, int.Parse(x)),
                ["-ClientCount"] = x => ClientCount = int.Parse(x),
                ["-TimeLimit"] = x => TimeLimit = TimeSpan.FromSeconds(int.Parse(x)),
                ["-MaxClientCount"] = x => MaxClientCount = int.Parse(x),
                ["-LoggingFolder"] = x => LoggingFolder = x,
            };

            _switches = new Dictionary<string, Action>
            {
                ["-?"] = () => _help = true,
                ["-Help"] = () => _help = true,
                ["-StoredProcedure"] = () => Operation = Operation.StoredProcedure,
                ["-BulkInsert"] = () => Operation = Operation.BulkInsert,
                ["-DataTable"] = () => Operation = Operation.DataTable,
                ["-Comparison"] = () => Operation = Operation.Comparison,
            };
        }

        public bool Help => _help ?? Operation == Operation.None;

        public Operation Operation { get; private set; } = Operation.None;

        public int BatchSize { get; private set; } = 100;

        public TimeSpan SampleRate { get; private set; } = TimeSpan.FromSeconds(5);

        public int ClientCount { get; private set; } = 1;

        public TimeSpan? TimeLimit { get; private set; }

        public int MaxClientCount { get; private set; } = 5;

        public string LoggingFolder { get; private set; }

        public static IOptions Process(IEnumerable<string> args)
        {
            return new Options().ProcessInternal(args);
        }

        public static void DisplayHelp()
        {
            var lines = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("SQL Bulk Insert Tests", $"Version {Assembly.GetExecutingAssembly().GetName().Version}."),
                new Tuple<string, string>(null, null),
                new Tuple<string, string>("-?", "Display help"),
                new Tuple<string, string>("-Help", "Display help"),
                new Tuple<string, string>(null, null),
                new Tuple<string, string>("Tests...", null),
                new Tuple<string, string>("-StoredProcedure", "Use SQL stored procedures"),
                new Tuple<string, string>("-BulkInsert", "Use ADO.NET SqlBulkCopy using IDataReader"),
                new Tuple<string, string>("-DataTable", "Use ADO.NET DataTable with SqlBulkCopy"),
                new Tuple<string, string>("-Comparison", "Performance comparison test between stored procedure and bulk insert, over a range of parameters."),
                new Tuple<string, string>(null, null),
                new Tuple<string, string>("Options...", null),
                new Tuple<string, string>("-BatchSize n", "Set batch size, only valid for -BulkInsert.  Default=100, Minimum=10"),
                new Tuple<string, string>("-ClientCount n", "Client count, (default = 1)"),
                new Tuple<string, string>("-TimeLimit n", "Test execute for n seconds, (default = off)"),
                new Tuple<string, string>("-MaxClientCount n", "Max client count for 'Comparison' test, (default = 5)"),
                new Tuple<string, string>("-LoggingFolder {file}", "Logging folder (optional)"),
                new Tuple<string, string>(null, null),
            };

            lines.ForEach(x => Console.WriteLine($"{x.Item1,-20} {x.Item2}"));
        }

        public void DisplayOptions()
        {
            var lines = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Operations", Operation.ToString()),
                new Tuple<string, string>("Batch size", BatchSize.ToString()),
                new Tuple<string, string>("Client count", ClientCount.ToString()),
                new Tuple<string, string>("Time limit", TimeLimit?.ToString() ?? "<not set>"),
            };

            lines.ForEach(x => Console.WriteLine($"{x.Item1,-12}: {x.Item2}"));
            Console.WriteLine();
        }

        private IOptions ProcessInternal(IEnumerable<string> args)
        {
            if (args == null || args.Count() == 0)
            {
                return new Options();
            }

            var stack = new Stack<string>(args.Reverse());
            while( stack.Count > 0 )
            {
                string cmd = stack.Pop();

                IEnumerable<string> detectedCmds = _commands.Keys.Concat(_switches.Keys)
                        .Where(x => x.Substring(0, Math.Min(x.Length, cmd.Length)).Equals(cmd, StringComparison.OrdinalIgnoreCase));

                if (detectedCmds.Count() != 1)
                {
                    throw new ArgumentException($"Unknown command {cmd}");
                }

                cmd = detectedCmds.First();
                if (_switches.TryGetValue(cmd, out Action switchCommand))
                {
                    switchCommand();
                    continue;
                }

                if (stack.Count == 0)
                {
                    throw new ArgumentException($"{cmd} requires a parameter");
                }

                if (_commands.TryGetValue(cmd, out Action<string> commandAction))
                {
                    commandAction(stack.Pop());
                }
            }

            if (Operation == Operation.Comparison && TimeLimit == null)
            {
                throw new ArgumentException("-Comparison requires -TimeLimit");
            }

            return this;
        }
    }
}
