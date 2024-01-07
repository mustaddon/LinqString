using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Tests;

public class TestSelectCount
{
    [Test]
    public async Task EfCountOnly()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Authors
            .Include(x => x.Books);

        var result = await query
            .Select("Books.Count()")
            .ToListAsync();

        var test = await query
            .Select(x => new
            {
                BooksCount = x.Books.Count(),
            })
            .ToListAsync();

        Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(JsonSerializer.Serialize(test)));
    }

    [Test]
    public async Task EfCountProp()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Authors
            .Include(x => x.Books);

        var result = await query
            .Select("AuthorId", "Books.Count()")
            .ToListAsync();

        var test = await query
            .Select(x => new
            {
                x.AuthorId,
                BooksCount = x.Books.Count(),
            })
            .ToListAsync();

        Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(JsonSerializer.Serialize(test)));
    }

    [Test]
    public async Task EfCountFull()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Authors
            .Include(x => x.Books);

        var result = await query
            .Select("Books", "Books.Count()")
            .ToListAsync();

        var test = await query
            .Select(x => new
            {
                x.Books,
                BooksCount = x.Books.Count(),
            })
            .ToListAsync();

        Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(JsonSerializer.Serialize(test)));
    }

    [Test]
    public async Task EfCountTrim()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Authors
            .Include(x => x.Books);

        var result = await query
            .Select("Books.Title", "Books.Count()")
            .ToListAsync();

        var test = await query
            .Select(x => new
            {
                Books = x.Books.Select(xx => new { xx.Title }),
                BooksCount = x.Books.Count(),
            })
            .ToListAsync();

        Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(JsonSerializer.Serialize(test)));
    }
}