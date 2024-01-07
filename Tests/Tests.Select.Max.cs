using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Tests;

public class TestSelectMax
{
    [Test]
    public async Task EfMax()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Authors
            .Include(x => x.Books);

        var result = await query
            .Select("Books.Max(BookId)")
            .ToListAsync();

        var test = await query
            .Select(x => new
            {
                BooksMaxBookId = x.Books.Max(xx => xx.BookId),
            })
            .ToListAsync();

        Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(JsonSerializer.Serialize(test)));
    }
}