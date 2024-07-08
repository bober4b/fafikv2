using Fafikv2.Configuration.BotConfig;
using Fafikv2.Services.OtherServices.Interfaces;

namespace Fafikv2.Services.OtherServices
{
    public class DatabaseContextQueueService : IDatabaseContextQueueService
    {
        private readonly DatabaseContextQueue _queue = new();

        public Task EnqueueDatabaseTask(Func<Task> task)
        {
            return _queue.Enqueue(task);
        }

        public  Task<T> EnqueueDatabaseTask<T>(Func<Task<T>> task)
        {
            return _queue.Enqueue(task);
        }

        public Task<Func<Task>> DequeueDatabaseTask(CancellationToken cancellationToken)
        {
            return _queue.DequeueAsync(cancellationToken);
        }

        public void DisplayQueueCount()
        {
            _queue.FuncNumberInQueue();
        }
    }
}
