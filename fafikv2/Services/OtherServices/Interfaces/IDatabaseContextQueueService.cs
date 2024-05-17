namespace Fafikv2.Services.OtherServices.Interfaces
{
    public interface IDatabaseContextQueueService
    {
        Task EnequeDatabaseTask(Func<Task> task);
        Task<Func<Task>> DequeueDatabaseTask(CancellationToken cancellationToken);
        void DisplayQueueCount();
    }
}
