using System.ComponentModel.DataAnnotations;

namespace Rz.TaskQueue.Server.DTO;

public class MessageReceipt
{
    public int MessageId { get; set; }

    [Required]
    public string Receipt { get; set; } = default!;
}
