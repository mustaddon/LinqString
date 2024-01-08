using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Tests;

public class TestSelectFirst
{
    [Test]
    public async Task EfFirst()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Authors
            .Include(x => x.Books);

        var result = await query
            .Select("Books.First(BookId)")
            .ToListAsync();

        var test = await query
            .Select(x => new
            {
                BooksFirstBookId = x.Books.Select(xx => xx.BookId).FirstOrDefault(),
            })
            .ToListAsync();

        Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(JsonSerializer.Serialize(test)));
    }

    [Test]
    public async Task EfFirstAll()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Authors
            .Include(x => x.Books);

        var result = await query
            .Select("Books.First()")
            .ToListAsync();

        var test = await query
            .Select(x => new
            {
                BooksFirst = x.Books.FirstOrDefault(),
            })
            .ToListAsync();

        Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(JsonSerializer.Serialize(test)));
    }
}