using Microsoft.EntityFrameworkCore;

namespace Tests;

public class TestGroupBy
{
    [Test]
    public void GroupBy()
    {
        var items = Enumerable.Range(0, 100).Select(x => x == 0 ? null : new
        {
            Even = x % 2 == 0,
            Prop1 = x,
            Obj = x == 1 ? null : new
            {
                Type = x % 5 == 0,
                Prop2 = "text" + x
            },
        }).ToList();

        var vars = new (string[], IEnumerable<object?>)[] {
            (["Even"], items.GroupBy(x => new { x?.Even }).ToList()),
            (["Even","Obj.Type"], items.GroupBy(x => new { x?.Even, x?.Obj?.Type }).ToList()),
        };

        var cache = new NeverExpiredCache();

        foreach (var (paths, test) in vars)
        {
            var cacheless = items.AsQueryable().GroupBy(paths).ToList();
            var cachable = items.AsQueryable().GroupBy(paths, cache).ToList();

            Assert.That(cacheless, Is.EqualTo(test).AsCollection);
            Assert.That(cachable, Is.EqualTo(test).AsCollection);

            cacheless = items.GroupBy(paths).ToList();
            cachable = items.GroupBy(paths, cache).ToList();

            Assert.That(cacheless, Is.EqualTo(test).AsCollection);
            Assert.That(cachable, Is.EqualTo(test).AsCollection);
        }

    }

#if NET6_0_OR_GREATER
    [Test]
    public async Task EfCore()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Books
            .Include(x => x.Author);

        var result = await query
            .GroupBy("Author.FirstName")
            .ToListAsync();

        var test = await query
            .GroupBy(x => x.Author!.FirstName)
            .ToListAsync();

        Assert.That(result, Is.EqualTo(test).AsCollection);
    }
#endif
}