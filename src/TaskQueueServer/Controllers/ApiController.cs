using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Rz.TaskQueue.Server.Controllers;

[ApiController]
[Route("/api/v1")]
public class ApiController : ControllerBase
{
    private readonly ILogger _logger;

    private readonly IDbContextFactory<PsqlContext> _dbContextFactory;

    public ApiController(ILogger<ApiController> logger, IDbContextFactory<PsqlContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    [HttpPost("queues")]
    public async Task<IActionResult> CreateQueueAsync([FromBody] string queueName)
    {
        var queue = new Queue(_dbContextFactory, queueName);
        await queue.CreateAsync();
        return NoContent();
    }

    [HttpDelete("queues/{queueName}")]
    public async Task<IActionResult> DeleteQueueAsync(string queueName)
    {
        var queue = new Queue(_dbContextFactory, queueName);
        await queue.DeleteAsync();
        return NoContent();
    }

    [HttpGet("queues/{queueName}/stat")]
    public async Task<IQueueStat> GetQueueStatAsync(string queueName)
    {
        var queue = new Queue(_dbContextFactory, queueName);
        return await queue.GetStatAsync();
    }

    [HttpPost("queues/{queueName}/in")]
    public async Task<IActionResult> PutMessageAsync(string queueName, [FromBody] string message)
    {
        var queue = new Queue(_dbContextFactory, queueName);
        await queue.PutMessageAsync(message);
        return NoContent();
    }

    //NOTE: It's not a HTTP GET because the function is not idempotent.
    [HttpPost("queues/{queueName}/out")]
    public Task<IQueueMessage?> GetMessageAsync(string queueName, [FromBody] int? lease = null)
    {
        var queue = new Queue(_dbContextFactory, queueName);
        return queue.GetMessageAsync(lease);
    }

    [HttpDelete("queues/{queueName}/messages/{msgId}")]
    public async Task<IActionResult> DeleteMessageAsync(string queueName, int msgId, [FromQuery] string receipt)
    {
        var queue = new Queue(_dbContextFactory, queueName);
        try
        {
            await queue.DeleteMessageAsync(msgId, receipt);
        }
        catch (InvalidQueueOperation)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("queues/{queueName}/messages/{msgId}/return")]
    public async Task<IActionResult> ReturnMessageAsync(string queueName, int msgId, [FromQuery] string receipt)
    {
        var queue = new Queue(_dbContextFactory, queueName);
        try
        {
            await queue.ReturnMessageAsync(msgId, receipt);
        }
        catch (InvalidQueueOperation)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("queues/{queueName}/messages/{msgId}/lease")]
    public async Task<IActionResult> ExtendMessageLeaseAsync(string queueName, int msgId, [FromQuery] string receipt, [FromBody] int? lease)
    {
        var queue = new Queue(_dbContextFactory, queueName);
        try
        {
            await queue.ExtendMessageLeaseAsync(msgId, receipt, lease);
        }
        catch (InvalidQueueOperation)
        {
            return NotFound();
        }
        return NoContent();
    }
}
