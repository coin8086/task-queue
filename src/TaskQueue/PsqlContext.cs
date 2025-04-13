using Microsoft.EntityFrameworkCore;
using Rz.TaskQueue.Models;

namespace Rz.TaskQueue;

public class PsqlContext : DbContext
{
    public PsqlContext(DbContextOptions<PsqlContext> options) : base(options) { }

    public DbSet<Message> Messages { get; set; }
}
