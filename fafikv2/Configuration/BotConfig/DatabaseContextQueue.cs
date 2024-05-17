using System;
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

        public async Task Enqueue(Func<Task> task)
        {

            _taskQueue.Enqueue(task);
            _signal.Release();

        }

        public async Task<Func<Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _taskQueue.TryDequeue(out var task);
            return task;
        }

        public void FuncNumberInQueue()
        {
            Console.WriteLine("Liczba tasków w kolejce: "+ _taskQueue.Count);
        }
    }
}
