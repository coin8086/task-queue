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
        _logger.LogDebug("Create queue '{name}'", queueName);
        var queue = new Queue(_dbContextFactory, queueName);
        await queue.CreateAsync();
        return NoContent();
    }

    [HttpDelete("queues/{queueName}")]
    public async Task<IActionResult> DeleteQueueAsync(string queueName)
    {
        _logger.LogDebug("Delete queue '{name}'", queueName);
        var queue = new Queue(_dbContextFactory, queueName);
        await queue.DeleteAsync();
        return NoContent();
    }

    [HttpGet("queues/{queueName}/stat")]
    public async Task<IQueueStat> GetQueueStatAsync(string queueName)
    {
        _logger.LogDebug("Get stat of queue '{name}'", queueName);
        var queue = new Queue(_dbContextFactory, queueName);
        return await queue.GetStatAsync();
    }

    [HttpPost("queues/{queueName}/in")]
    public async Task<IActionResult> PutMessageAsync(string queueName, [FromBody] string message)
    {
        //TODO: Show first n chars of message in log?
        _logger.LogDebug("Put message in queue '{name}'", queueName);
        var queue = new Queue(_dbContextFactory, queueName);
        await queue.PutMessageAsync(message);
        return NoContent();
    }

    //NOTE: It's not a HTTP GET because the function is not idempotent.
    [HttpPost("queues/{queueName}/out")]
    public async Task<IQueueMessage?> GetMessageAsync(string queueName, [FromBody] int? lease = null)
    {
        var queue = new Queue(_dbContextFactory, queueName);
        var msg = await queue.GetMessageAsync(lease);
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            if (msg == null)
            {
                _logger.LogDebug("Got no message from queue '{name}'", queueName);
            }
            else
            {
                _logger.LogDebug("Got message ({id}, {receipt}) from queue '{name}'", msg?.Id, msg?.Receipt, queueName);
            }
        }
        return msg;
    }

    [HttpDelete("queues/{queueName}/messages/{msgId}")]
    public async Task<IActionResult> DeleteMessageAsync(string queueName, int msgId, [FromQuery] string receipt)
    {
        _logger.LogDebug("Delete message ({id}, {receipt}) from queue '{name}'", msgId, receipt, queueName);
        var queue = new Queue(_dbContextFactory, queueName);
        try
        {
            await queue.DeleteMessageAsync(msgId, receipt);
        }
        catch (InvalidQueueOperation)
        {
            _logger.LogDebug("Message ({id}, {receipt}) is not found in queue '{name}'", msgId, receipt, queueName);
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("queues/{queueName}/messages/{msgId}/return")]
    public async Task<IActionResult> ReturnMessageAsync(string queueName, int msgId, [FromQuery] string receipt)
    {
        _logger.LogDebug("Return message ({id}, {receipt}) to queue '{name}'", msgId, receipt, queueName);
        var queue = new Queue(_dbContextFactory, queueName);
        try
        {
            await queue.ReturnMessageAsync(msgId, receipt);
        }
        catch (InvalidQueueOperation)
        {
            _logger.LogDebug("Message ({id}, {receipt}) is not found in queue '{name}'", msgId, receipt, queueName);
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("queues/{queueName}/messages/{msgId}/lease")]
    public async Task<IActionResult> ExtendMessageLeaseAsync(string queueName, int msgId, [FromQuery] string receipt, [FromBody] int? lease)
    {
        _logger.LogDebug("Extend lease of message ({id}, {receipt}) in queue '{name}'", msgId, receipt, queueName);
        var queue = new Queue(_dbContextFactory, queueName);
        try
        {
            await queue.ExtendMessageLeaseAsync(msgId, receipt, lease);
        }
        catch (InvalidQueueOperation)
        {
            _logger.LogDebug("Message ({id}, {receipt}) is not found in queue '{name}'", msgId, receipt, queueName);
            return NotFound();
        }
        return NoContent();
    }
}
