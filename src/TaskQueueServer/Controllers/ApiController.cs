using Microsoft.AspNetCore.Mvc;

namespace Rz.TaskQueue.Server.Controllers;

[ApiController]
[Route("/api/v1")]
public class ApiController : ControllerBase
{
    [HttpPost("queues")]
    public IActionResult CreateQueue([FromBody] string queueName)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("queues/{queueName}")]
    public IActionResult DeleteQueue(string queueName)
    {
        throw new NotImplementedException();
    }

    [HttpPost("queues/{queueName}/in")]
    public IActionResult PutMessage(string queueName, [FromBody] string message)
    {
        throw new NotImplementedException();
    }

    //NOTE: It's not a HTTP GET because the function is not idempotent.
    [HttpPost("queues/{queueName}/out")]
    public IQueueMessage GetMessage(string queueName, [FromBody] int? lease = null)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("queues/{queueName}/messages/{msgId}")]
    public IActionResult DeleteMessage(string queueName, int msgId, [FromBody] DTO.MessageReceipt receipt)
    {
        receipt.MessageId = msgId;
        throw new NotImplementedException();
    }

    [HttpPost("queues/{queueName}/messages/{msgId}/return")]
    public IActionResult ReturnMessage(string queueName, int msgId, [FromBody] DTO.MessageReceipt receipt)
    {
        receipt.MessageId = msgId;
        throw new NotImplementedException();
    }

    [HttpPost("queues/{queueName}/messages/{msgId}/lease")]
    public IActionResult ExtendMessageLease(string queueName, int msgId, [FromBody] DTO.MessageLease lease)
    {
        lease.MessageId = msgId;
        throw new NotImplementedException();
    }
}
