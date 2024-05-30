namespace Fafikv2.Services.OtherServices.Interfaces
{
    public interface IDatabaseContextQueueService
    {
        Task EnqueueDatabaseTask(Func<Task> task);
        Task<T> EnqueueDatabaseTask<T>(Func<Task<T>> task);
        Task<Func<Task>> DequeueDatabaseTask(CancellationToken cancellationToken);
        void DisplayQueueCount();
    }
}
