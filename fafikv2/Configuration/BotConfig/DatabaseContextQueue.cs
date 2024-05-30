﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fafikv2.Configuration.BotConfig
{
    public class DatabaseContextQueue
    {
        private readonly ConcurrentQueue<Func<Task>> _taskQueue = new();
        private readonly SemaphoreSlim _signal=new(0);

        public Task Enqueue(Func<Task> task)
        {

            _taskQueue.Enqueue(task);
            _signal.Release();
            return Task.CompletedTask;

        }

        public Task<T> Enqueue<T>(Func<Task<T>> task)
        {
            var tcs = new TaskCompletionSource<T>();
            _taskQueue.Enqueue(async () =>
            {
                try
                {
                    T result= await task().ConfigureAwait(false);
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    
                    tcs.SetException(ex);
                }
            });
            _signal.Release();
            return tcs.Task;
        }

        public async Task<Func<Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
            _taskQueue.TryDequeue(out var task);
            return task;
        }

        public void FuncNumberInQueue()
        {
            Console.WriteLine("Liczba tasków w kolejce: "+ _taskQueue.Count);
        }
    }
}
