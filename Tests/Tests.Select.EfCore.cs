using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Tests;

public class TestSelectEfCore
{

    [Test]
    public async Task Test1()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Books
            .Include(x => x.Author);

        var result = await query
            .Select("Author.BirthDate", "Author.FirstName", "Title")
            .ToListAsync();

        var test = await query
            .Select(x => new
            {
                Author = x.Author == null ? null : new
                {
                    x.Author.BirthDate,
                    x.Author.FirstName
                },
                x.Title,
            })
            .ToListAsync();

        Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(JsonSerializer.Serialize(test)));
    }

    [Test]
    public async Task Test2()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Authors
            .Include(x => x.Books);

        var result = await query
            .Select("Books.Title", "FirstName")
            .ToListAsync();

        var test = await query
            .Select(x => new
            {
                Books = x.Books.Select(xx => new
                {
                    xx.Title
                }),
                x.FirstName,
            })
            .ToListAsync();

        Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(JsonSerializer.Serialize(test)));
    }

    [Test]
    public async Task Test3()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Authors
            .Include(x => x.Books);

        var result = await query
            .Select("Books", "FirstName")
            .ToListAsync();

        var test = await query
            .Select(x => new
            {
                x.Books,
                x.FirstName,
            })
            .ToListAsync();

        Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(JsonSerializer.Serialize(test)));
    }
}