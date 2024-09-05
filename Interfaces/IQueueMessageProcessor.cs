
using Azure.Storage.Queues.Models;

public interface IQueueMessageProcessor
{
    Task ProcessMessageAsync(QueueMessage message);
}