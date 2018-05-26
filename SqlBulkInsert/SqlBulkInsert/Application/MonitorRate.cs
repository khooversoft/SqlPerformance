// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;

namespace SqlBulkInsert
{
    internal class MonitorRate : IDisposable
    {
        private static TimeSpan _period = TimeSpan.FromSeconds(1);
        private RateDetail _currentRateDetail;
        private Timer _timer;
        private int _flushLock = 0;

        public MonitorRate(MonitorReport report, string name)
        {
            Report = report;
            Name = name;

            _timer = new Timer(x => Flush(), null, _period, _period);
            _currentRateDetail = new RateDetail(name);
        }

        public MonitorReport Report { get; }

        public string Name { get; }

        public void IncrementNew(int value = 1)
        {
            Verify();

            _currentRateDetail.AddNew(value);
        }

        public void IncrementBatch(int value = 1)
        {
            Verify();

            _currentRateDetail.AddBatch(value);
        }

        public void IncrementError(int value = 1)
        {
            Verify();

            _currentRateDetail.AddError(value);
        }

        public void IncrementError(string errorMessage)
        {
            Verify();

            _currentRateDetail.AddError(errorMessage);
        }

        public void AddRetry(int value = 1)
        {
            Verify();

            _currentRateDetail.AddRetryCount(value);
        }

        public void Dispose()
        {
            Timer timer = Interlocked.Exchange(ref _timer, null);
            timer?.Dispose();

            if (timer != null)
            {
                Flush();
            }
        }

        private void Verify()
        {
            if (_timer == null)
            {
                throw new InvalidOperationException("Monitor is not running");
            }
        }

        private void Flush()
        {
            // Only allow one thread in (non blocking)
            int currentLock = Interlocked.CompareExchange(ref _flushLock, 1, 0);
            if (currentLock == 1)
            {
                return;
            }

            try
            {
                RateDetail current = Interlocked.Exchange(ref _currentRateDetail, new RateDetail(Name));
                current.Stop();
                Report.Enqueue(current);
                return;
            }
            finally
            {
                Interlocked.Exchange(ref _flushLock, 0);
            }
        }
    }
}
