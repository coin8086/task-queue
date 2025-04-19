using System.ComponentModel.DataAnnotations;

namespace Rz.TaskQueue.Server.DTO;

public class MessageLease : MessageReceipt
{
    [Range(1, int.MaxValue)]
    public int? Lease { get; set; }
}
