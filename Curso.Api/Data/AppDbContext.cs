using Microsoft.EntityFrameworkCore;
using Curso.Api.Models;

namespace Curso.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
}
