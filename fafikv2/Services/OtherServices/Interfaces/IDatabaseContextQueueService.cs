namespace Fafikv2.Services.OtherServices.Interfaces
{
    public interface IDatabaseContextQueueService
    {
        Task EnqueueDatabaseTask(Func<Task> task);
        Task<Func<Task>> DequeueDatabaseTask(CancellationToken cancellationToken);
        void DisplayQueueCount();
    }
}
