// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBulkInsert
{
    /// <summary>
    /// Run specific scenarios, specifically the comparison between number of clients and batch sizes
    /// </summary>
    internal class ActionManager
    {
        private readonly ILifetimeScope _container;
        private readonly IOptions _options;
        private readonly ILogging _logging;

        public ActionManager(ILifetimeScope container, IOptions options, ILogging logging)
        {
            _container = container;
            _options = options;
            _logging = logging;
        }

        public async Task Run(IEnumerable<IAction> actions)
        {
            await Launch(async token => await RunActions(_container, actions, token));
        }

        public async Task RunComparisons()
        {
            await Launch(async token => await InternalRunComparisons(token));
        }

        public async Task InternalRunComparisons(CancellationToken token)
        {
            TestOptions testOption;

            IOptions options = _container.Resolve<IOptions>();

            _logging.Log();
            _logging.Log(() => "Running comparison tests, clients 1 to 11 by 2, batch sizes 500 to 2000 by 500");
            _logging.Log();

            for (int clientCount = 1; clientCount <= _options.MaxClientCount; clientCount += 2)
            {
                for (int batchSize = 500; batchSize <= 5000; batchSize += 500)
                {
                    _logging.Log();
                    _logging.Log(() => $"Comparison: Client count={clientCount}");
                    _logging.Log(() => $"Comparison: Batch size={batchSize}");

                    testOption = new TestOptions(Operation.StoredProcedure, batchSize, clientCount, options.TimeLimit);
                    using (ILifetimeScope scopedContainer = _container.BeginLifetimeScope(builder => builder.Register(x => testOption).As<IOptions>().InstancePerLifetimeScope()))
                    {
                        var actions = new IAction[]
                        {
                                scopedContainer.Resolve<ResetAction>(),
                                scopedContainer.Resolve<StoredProcedureBulkCopy>()
                        };

                        await RunActions(scopedContainer, actions, token);
                    }

                    testOption = new TestOptions(Operation.StoredProcedure, batchSize, clientCount, options.TimeLimit);
                    using (ILifetimeScope scopedContainer = _container.BeginLifetimeScope(builder => builder.Register(x => testOption).As<IOptions>().InstancePerLifetimeScope()))
                    {
                        var actions = new IAction[]
                        {
                                scopedContainer.Resolve<ResetAction>(),
                                scopedContainer.Resolve<SqlBulkCopyAction>()
                        };

                        await RunActions(scopedContainer, actions, token);
                    }

                    testOption = new TestOptions(Operation.DataTable, batchSize, clientCount, options.TimeLimit);
                    using (ILifetimeScope scopedContainer = _container.BeginLifetimeScope(builder => builder.Register(x => testOption).As<IOptions>().InstancePerLifetimeScope()))
                    {
                        var actions = new IAction[]
                        {
                                scopedContainer.Resolve<ResetAction>(),
                                scopedContainer.Resolve<DataTableAction>()
                        };

                        await RunActions(scopedContainer, actions, token);
                    }
                }
            }
        }

        private async Task Launch(Func<CancellationToken, Task> run)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            Task runTask = null;
            try
            {
                runTask = Task.Run(async () => await run(tokenSource.Token));
            }
            catch (TaskCanceledException) { }
            catch (AggregateException) { }

            Console.WriteLine();
            Console.WriteLine("Starting... Press <return> to quit");

            while (!runTask.IsCompleted)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            }

            tokenSource.Cancel();
            await runTask;
            Console.WriteLine();
        }

        private static async Task RunActions(ILifetimeScope container, IEnumerable<IAction> actions, CancellationToken outterToken)
        {
            CancellationTokenSource innerTokenSource = new CancellationTokenSource();
            CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(outterToken, innerTokenSource.Token);
            Timer timer = null;

            IOptions options = container.Resolve<IOptions>();

            try
            {
                foreach (var action in actions)
                {
                    if (options.TimeLimit != null)
                    {
                        timer?.Dispose();
                        timer = new Timer(x => innerTokenSource.Cancel(), null, (TimeSpan)options.TimeLimit, TimeSpan.FromMilliseconds(-1));
                    }

                    await action.Process(tokenSource.Token);
                }
            }
            catch (TaskCanceledException) { }
            catch (AggregateException) { }
            finally
            {
                timer?.Dispose();
            }
        }
    }
}
