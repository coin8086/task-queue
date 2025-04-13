using Microsoft.EntityFrameworkCore;
using Rz.TaskQueue.Models;

namespace Rz.TaskQueue;

public class PsqlContext : DbContext
{
    public DbSet<Message> Messages { get; set; }
}
