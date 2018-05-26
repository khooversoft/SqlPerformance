// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBulkInsert
{
    class Program
    {
        private const int _ok = 0;
        private const int _error = 1;
        private const string _lifetimeScopeTag = "main";

        static async Task Main(string[] args)
        {
            await new Program().Run(args);
        }

        private async Task<int> Run(string[] args)
        {
            Console.WriteLine("SQL Bulk Insert Tests", $"Version {Assembly.GetExecutingAssembly().GetName().Version}.");
            Console.WriteLine();

            try
            {
                IOptions options = Options.Process(args);
                if (options.Help)
                {
                    Options.DisplayHelp();
                    return _ok;
                }

                options.DisplayOptions();

                using (ILifetimeScope container = CreateContainer(options).BeginLifetimeScope(_lifetimeScopeTag))
                {
                    ILogging logging = container.Resolve<ILogging>();

                    switch (options.Operation)
                    {
                        case Operation.StoredProcedure:
                            var storedProcedureActions = new IAction[]
                            {
                                container.Resolve<ResetAction>(),
                                container.Resolve<StoredProcedureBulkCopy>()
                            };

                            await container.Resolve<ActionManager>(new TypedParameter(typeof(ILifetimeScope), container)).Run(storedProcedureActions);
                            ReportMetrics(container);
                            break;

                        case Operation.BulkInsert:
                            var bulkCopyActions = new IAction[]
                            {
                                container.Resolve<ResetAction>(),
                                container.Resolve<SqlBulkCopyAction>()
                            };

                            await container.Resolve<ActionManager>(new TypedParameter(typeof(ILifetimeScope), container)).Run(bulkCopyActions);
                            ReportMetrics(container);
                            break;

                        case Operation.DataTable:
                            var dataTableActions = new IAction[]
                            {
                                container.Resolve<ResetAction>(),
                                container.Resolve<DataTableAction>()
                            };

                            await container.Resolve<ActionManager>(new TypedParameter(typeof(ILifetimeScope), container)).Run(dataTableActions);
                            ReportMetrics(container);
                            break;

                        case Operation.Comparison:
                            await container.Resolve<ActionManager>(new TypedParameter(typeof(ILifetimeScope), container)).RunComparisons();
                            ReportMetrics(container);
                            break;
                    }
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return _error;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhanded exception:{ex.Message}, {ex.ToString()}");
                return _error;
            }

            return _ok;
        }

        private IContainer CreateContainer(IOptions options)
        {
            var builder = new ContainerBuilder();

            builder.Register(x => options).As<IOptions>().InstancePerLifetimeScope();
            builder.RegisterInstance(new Configuration()).As<IConfiguration>();

            builder.RegisterType<Logging>().As<ILogging>().InstancePerMatchingLifetimeScope(_lifetimeScopeTag);
            builder.RegisterType<ActionManager>();

            builder.RegisterType<ResetAction>().InstancePerLifetimeScope();
            builder.RegisterType<StoredProcedureBulkCopy>().InstancePerLifetimeScope();
            builder.RegisterType<SqlBulkCopyAction>().InstancePerLifetimeScope();
            builder.RegisterType<DataTableAction>().InstancePerLifetimeScope();

            builder.RegisterType<TestMetricManager>().As<ITestMetricManager>().SingleInstance();

            return builder.Build();
        }

        private void ReportMetrics(ILifetimeScope container)
        {
            Console.WriteLine("Metric Report...");
            ITestMetricManager manager = container.Resolve<ITestMetricManager>();
            ILogging logging = container.Resolve<ILogging>();

            var maxRecord = manager
                .Select((x, i) => new { r = x, index = i })
                .OrderBy(x => x.r.Tps).Last();

            foreach (TestMetric metric in manager.OrderBy(x => x.Name).ThenBy(x => x.Tps))
            {
                logging.Log(() => $"{metric}, {(maxRecord.r.Tps == metric.Tps ? "Max" : string.Empty)}");
            }

            ByTestName(manager, logging);
            ByBatchSize(manager, logging);
        }

        private void ByTestName(ITestMetricManager manager, ILogging logging)
        {
            logging.Log();
            logging.Log(() => "By test name...");

            var tests = manager
                .GroupBy(x => x.Name);

            foreach (var test in tests)
            {
                logging.Log();
                logging.Log(() => $"Name={test.Key}");

                var subTests = manager
                    .Where(x => x.Name == test.Key)
                    .OrderBy(x => x.Tps);

                foreach (TestMetric metric in subTests)
                {
                    logging.Log(() => metric.ToString());
                }
            }
        }

        private void ByBatchSize(ITestMetricManager manager, ILogging logging)
        {
            logging.Log();
            logging.Log(() => "By batch size...");

            var tests = manager
                .GroupBy(x => x.BatchSize);

            foreach (var test in tests)
            {
                logging.Log();
                logging.Log(() => $"Batch Size={test.Key}");

                var subTests = manager
                    .Where(x => x.BatchSize == test.Key)
                    .OrderBy(x => x.Tps);

                foreach (TestMetric metric in subTests)
                {
                    logging.Log(() => metric.ToString());
                }
            }
        }
    }
}
