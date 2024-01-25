namespace BlogApp.Data;

using Models;
using Mappings;
using Microsoft.EntityFrameworkCore;

public class BlogDataContext(DbContextOptions<BlogDataContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=localhost,1433;
                        Database=Blog;
                        User ID=sa;
                        Password=MiguelPassword@192; 
                        Trust Server Certificate = true");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CategoryMap());
        modelBuilder.ApplyConfiguration(new UserMap());
        modelBuilder.ApplyConfiguration(new PostMap());
    }
}