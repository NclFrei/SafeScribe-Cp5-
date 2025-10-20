using Microsoft.EntityFrameworkCore;
using SafeScribe.Domain.Models;

namespace SafeScribe.Infrastructure.Data;

public class SafeScribeContext : DbContext
{
    public SafeScribeContext(DbContextOptions<SafeScribeContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
    public DbSet<Note> Notes { get; set; }
}
