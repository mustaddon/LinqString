using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Tests;

public class TestSelectAverage
{
    [Test]
    public async Task EfAverage()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Authors
            .Include(x => x.Books);

        var result = await query
            .Select("Books.Average(BookId)")
            .ToListAsync();

        var test = await query
            .Select(x => new
            {
                BooksAverageBookId = x.Books.Average(xx => xx.BookId),
            })
            .ToListAsync();

        Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(JsonSerializer.Serialize(test)));
    }
}