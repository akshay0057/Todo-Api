using Microsoft.EntityFrameworkCore;
using TodoApiProject.Data.Entities;

namespace TodoApiProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<UserEntity> Users { get; set; } = null!;
        public DbSet<TodoItem> TodoItems { get; set; } = null!;
    }
}
