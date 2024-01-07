using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Tests;

public class TestSelectSum
{
    [Test]
    public async Task EfSum()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Authors
            .Include(x => x.Books);

        var result = await query
            .Select("Books.Sum(BookId)")
            .ToListAsync();

        var test = await query
            .Select(x => new
            {
                BooksSumBookId = x.Books.Sum(xx => xx.BookId),
            })
            .ToListAsync();

        Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(JsonSerializer.Serialize(test)));
    }
}