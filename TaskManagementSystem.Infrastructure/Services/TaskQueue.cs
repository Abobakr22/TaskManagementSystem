using System.Threading.Channels;

namespace TaskManagementSystem.Infrastructure.Services;

public class TaskQueue
{
    private readonly Channel<Guid> _channel;

    public TaskQueue()
    {
        var options = new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<Guid>(options);
    }

    public async ValueTask QueueTaskAsync(Guid taskId)
    {
        await _channel.Writer.WriteAsync(taskId);
    }

    public IAsyncEnumerable<Guid> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}