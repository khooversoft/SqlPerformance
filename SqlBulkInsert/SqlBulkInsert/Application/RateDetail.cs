// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SqlBulkInsert
{
    internal class RateDetail
    {
        public RateDetail(string name)
        {
            Name = name;
            StartTime = DateTimeOffset.UtcNow;
        }

        public string Name { get; }

        public DateTimeOffset StartTime { get; private set; }

        public DateTimeOffset? StopTime { get; private set; }

        public TimeSpan Time { get; }

        public int NewCount { get; private set; }

        public int BatchCount { get; private set; }

        public int ErrorCount { get; private set; }

        public int RetryCount { get; private set; }

        public string LastErrorMessage { get; private set; }

        public double TpsRate
        {
            get
            {
                int count = NewCount;
                if (count == 0)
                {
                    return 0;
                }

                DateTimeOffset endTime = StopTime ?? DateTimeOffset.UtcNow;
                TimeSpan delta = endTime - StartTime;

                return count / delta.TotalSeconds;
            }
        }

        public double TpsReadRate
        {
            get
            {
                if (BatchCount == 0)
                {
                    return 0;
                }

                DateTimeOffset endTime = StopTime ?? DateTimeOffset.UtcNow;
                TimeSpan delta = endTime - StartTime;

                return BatchCount / delta.TotalSeconds;
            }
        }

        public double TpsNewRate
        {
            get
            {
                if (NewCount == 0)
                {
                    return 0;
                }

                DateTimeOffset endTime = StopTime ?? DateTimeOffset.UtcNow;
                TimeSpan delta = endTime - StartTime;

                return NewCount / delta.TotalSeconds;
            }
        }

        public void AddNew(int value)
        {
            NewCount += value;
        }

        public void AddBatch(int value)
        {
            BatchCount += value;
        }

        public void AddError(int value)
        {
            ErrorCount += value;
        }

        public void AddError(string errorMessage)
        {
            ErrorCount++;
            LastErrorMessage = errorMessage ?? LastErrorMessage;
        }

        public void AddRetryCount(int value)
        {
            RetryCount += value;
        }

        public void Stop()
        {
            StopTime = DateTimeOffset.UtcNow;
        }

        public void Add(RateDetail rateDetail)
        {
            AddNew(rateDetail.NewCount);
            AddBatch(rateDetail.BatchCount);
            AddError(rateDetail.ErrorCount);
            AddRetryCount(rateDetail.RetryCount);
            LastErrorMessage = rateDetail.LastErrorMessage ?? LastErrorMessage;

            if (rateDetail.StartTime < StartTime)
            {
                StartTime = rateDetail.StartTime;
            }
        }
    }
}