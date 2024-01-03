using Microsoft.EntityFrameworkCore;

namespace Tests;

public class TestOrderBy
{
    [Test]
    public void OrderBy()
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
            (["Even","Obj.Prop2"], items.OrderBy(x=> x?.Even).ThenBy(x=>x?.Obj?.Prop2).ToList()),
            (["Even",">Obj.Prop2"], items.OrderBy(x=>x?.Even).ThenByDescending(x=>x?.Obj?.Prop2).ToList()),
            ([">Even","Obj.Prop2"], items.OrderByDescending(x=>x?.Even).ThenBy(x=>x?.Obj?.Prop2).ToList()),
            ([">Even",">Obj.Prop2"], items.OrderByDescending(x=>x?.Even).ThenByDescending(x=>x?.Obj?.Prop2).ToList()),
            (["Even","Obj.Type",">Prop1"], items.OrderBy(x=>x?.Even).ThenBy(x=>x?.Obj?.Type).ThenByDescending(x=>x?.Prop1).ToList()),
        };

        var cache = new NeverExpiredCache();

        foreach (var (paths, test) in vars)
        {
            var cacheless = items.AsQueryable().OrderBy(paths).ToList();
            var cachable = items.AsQueryable().OrderBy(paths, cache).ToList();

            Assert.That(cacheless, Is.EqualTo(test).AsCollection);
            Assert.That(cachable, Is.EqualTo(test).AsCollection);

            cacheless = items.OrderBy(paths).ToList();
            cachable = items.OrderBy(paths, cache).ToList();

            Assert.That(cacheless, Is.EqualTo(test).AsCollection);
            Assert.That(cachable, Is.EqualTo(test).AsCollection);
        }

    }

    
    [Test]
    public async Task EfCore()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Books
            .Include(x => x.Author);

        var result = await query
            .OrderBy("Author.FirstName", ">BookId")
            .ToListAsync();

        var test = await query
            .OrderBy(x => x.Author!.FirstName).ThenByDescending(x=>x.BookId)
            .ToListAsync();


        Assert.That(result, Is.EqualTo(test).AsCollection);
    }
}