using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Tests;

public class TestSelect
{
    [Test]
    public void Select()
    {
        var items = Enumerable.Range(0, 10).Select(x => x == 0 ? null : new
        {
            Prop1 = x,
            Prop2 = x == 1 ? null : new
            {
                Prop21 = x,
                Prop22 = $"text-{x}",
            },
            Prop3 = x == 2 ? null : Enumerable.Range(0, 5).Select(xx => xx == 0 ? null : new
            {
                Prop31 = xx + x * 1000,
                Prop32 = $"text-{x}-{xx}",
            })
        }).ToList();

        var cache = new NeverExpiredCache();
        var props = new[] { "Prop1", "Prop2.Prop22", "Prop3.Prop32" };
        var test = items.Select(x => x == null ? null : new
        {
            x.Prop1,
            Prop2 = x.Prop2 == null ? null : new { x.Prop2.Prop22 },
            Prop3 = x.Prop3 == null ? null : x.Prop3.Select(xx => xx == null ? null : new { xx.Prop32 }),
        }).ToList();

        // IQueryable
        var cacheless = items.AsQueryable().Select(props).ToList();
        var cachable = items.AsQueryable().Select(props, cache).ToList();

        Assert.That(JsonSerializer.Serialize(cacheless), Is.EqualTo(JsonSerializer.Serialize(test)));
        Assert.That(JsonSerializer.Serialize(cachable), Is.EqualTo(JsonSerializer.Serialize(test)));

        // IEnumerable
        cacheless = items.Select(props).ToList();
        cachable = items.Select(props, cache).ToList();

        Assert.That(JsonSerializer.Serialize(cacheless), Is.EqualTo(JsonSerializer.Serialize(test)));
        Assert.That(JsonSerializer.Serialize(cachable), Is.EqualTo(JsonSerializer.Serialize(test)));
    }

    [Test]
    public void Ñascade()
    {
        var items = Enumerable.Range(0, 10).Select(x => x == 0 ? null : new
        {
            Prop1 = x,
            Prop2 = x == 1 ? null : new
            {
                Prop21 = x,
                Prop22 = $"text-{x}",
            },
            Prop3 = x == 2 ? null : Enumerable.Range(0, 5).Select(xx => xx == 0 ? null : new
            {
                Prop31 = xx + x * 1000,
                Prop32 = $"text-{x}-{xx}",
            })
        }).ToList();

        var cache = new NeverExpiredCache();
        var first = new[] { "Prop1", "Prop2.Prop22", "Prop3.Prop32" };
        var second = new[] { "Prop1", "Prop2.Prop22" };
        var third = new[] { "Prop2.Prop22" };

        var test = items.Select(x => x == null ? null : new
        {
            Prop2 = x.Prop2 == null ? null : new { x.Prop2.Prop22 },
        }).ToList();

        // IQueryable
        var cacheless = items.AsQueryable().Select(first).Select(second).Select(third).ToList();
        var cachable = items.AsQueryable().Select(first, cache).Select(second, cache).Select(third, cache).ToList();

        Assert.That(JsonSerializer.Serialize(cacheless), Is.EqualTo(JsonSerializer.Serialize(test)));
        Assert.That(JsonSerializer.Serialize(cachable), Is.EqualTo(JsonSerializer.Serialize(test)));

        // IEnumerable
        cacheless = items.Select(first).Select(second).Select(third).ToList();
        cachable = items.Select(first, cache).Select(second, cache).Select(third, cache).ToList();

        Assert.That(JsonSerializer.Serialize(cacheless), Is.EqualTo(JsonSerializer.Serialize(test)));
        Assert.That(JsonSerializer.Serialize(cachable), Is.EqualTo(JsonSerializer.Serialize(test)));
    }


    [Test]
    public async Task EfCore1()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Books
            .Include(x => x.Author);

        var result = await query
            .Select("Author.BirthDate", "Author.FirstName", "Title")
            .ToListAsync();

        var test = await query
            .Select(x => new { 
                Author = x.Author == null ? null : new {
                    x.Author.BirthDate,
                    x.Author.FirstName
                },
                x.Title,
            })
            .ToListAsync();

        Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(JsonSerializer.Serialize(test)));
    }

    [Test]
    public async Task EfCore2()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Authors
            .Include(x => x.Books);

        var result = await query
            .Select("Books.Title", "FirstName")
            .ToListAsync();

        var test = await query
            .Select(x => new {
                Books = x.Books.Select(xx=> new { 
                    xx.Title
                }),
                x.FirstName,
            })
            .ToListAsync();

        Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(JsonSerializer.Serialize(test)));
    }
}