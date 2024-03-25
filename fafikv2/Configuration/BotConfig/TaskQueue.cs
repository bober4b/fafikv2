using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fafikv2.Configuration.BotConfig
{
    public class TaskQueue
    {
        private readonly ConcurrentQueue<Func<Task>> _TaskQueue = new ConcurrentQueue<Func<Task>>();
        private readonly SemaphoreSlim _signal=new SemaphoreSlim(0);

        public async Task Enqueue(Func<Task> task)
        {

            _TaskQueue.Enqueue(task);
            _signal.Release();

        }

        public async Task<Func<Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _TaskQueue.TryDequeue(out var task);
            return task;
        }
    }
}
