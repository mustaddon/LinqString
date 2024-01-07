using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Tests;

public class TestSelectMin
{
    [Test]
    public async Task EfMin()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Authors
            .Include(x => x.Books);

        var result = await query
            .Select("Books.Min(BookId)")
            .ToListAsync();

        var test = await query
            .Select(x => new
            {
                BooksMinBookId = x.Books.Min(xx => xx.BookId),
            })
            .ToListAsync();

        Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(JsonSerializer.Serialize(test)));
    }
}