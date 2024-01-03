using Microsoft.EntityFrameworkCore;

namespace Tests;

internal class EfCoreContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(@"Data Source=..\..\..\sqlite.db;");
    }

    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }


    internal async Task Init()
    {
        Database.EnsureCreated();

        await Books.AddAsync(new()
        {
            Title = "Authorless"
        });

        await Authors.AddRangeAsync(new List<Author>
        {
            new ()
            {
                FirstName ="Carson",
                LastName ="Alexander",
                BirthDate = DateTime.Parse("1985-09-01"),
                Books = new()
                {
                    new() { Title = "Introduction to Machine Learning"},
                    new() { Title = "Advanced Topics on Machine Learning"},
                    new() { Title = "Introduction to Computing"}
                }
            },
            new ()
            {
                FirstName ="Meredith",
                LastName ="Alonso",
                BirthDate = DateTime.Parse("1970-09-01"),
                Books = new()
                {
                    new() { Title = "Introduction to Microeconomics"}
                }
            },
            new ()
            {
                FirstName ="Arturo",
                LastName ="Anand",
                BirthDate = DateTime.Parse("1963-09-01"),
                Books = new()
                {
                    new() { Title = "Calculus I"},
                    new() { Title = "Calculus II"}
                }
            }
        });

        await SaveChangesAsync();
    }
}

internal class Author
{
    public int AuthorId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime BirthDate { get; set; }
    public List<Book> Books { get; set; } = default!;
}

internal class Book
{
    public int BookId { get; set; }
    public string Title { get; set; } = default!;
    public int? AuthorId { get; set; }
    public Author? Author { get; set; }
}
