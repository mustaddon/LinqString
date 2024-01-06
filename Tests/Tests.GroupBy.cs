using Microsoft.EntityFrameworkCore;
using System;

namespace Tests;

public class TestGroupBy
{
    static readonly Random _rnd = new();

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
            Objs = x == 2 ? null : Enumerable.Range(0, _rnd.Next(0, 10)).Select(xx => xx == 0 ? null : new
            {
                Prop3 = $"text-{x}-{xx}",
                Date1 = xx == 1 ? null : (DateTime?)DateTime.Today.AddDays(_rnd.Next(1, 10)),
                Date2 = xx == 1 ? null : (DateTimeOffset?)new DateTimeOffset(DateTime.Today.AddDays(_rnd.Next(1, 10))),
                Num1 = xx == 1 ? null : (byte?)_rnd.Next(1,10),
                Num2 = xx == 1 ? null : (short?)_rnd.Next(1, 10),
                Num3 = xx == 1 ? null : (ushort?)_rnd.Next(1, 10),
                Num4 = xx == 1 ? null : (int?)_rnd.Next(1, 10),
                Num5 = xx == 1 ? null : (uint?)_rnd.Next(1, 10),
                Num6 = xx == 1 ? null : (long?)_rnd.Next(1, 10),
                Num7 = xx == 1 ? null : (float?)_rnd.NextDouble(),
                Num8 = xx == 1 ? null : (double?)_rnd.NextDouble(),
                Num9 = xx == 1 ? null : (decimal?)_rnd.NextDouble(),
                Obj = xx == 1 ? null : new
                {
                    Prop4 = _rnd.Next(0, 10),
                },
                Objs = Enumerable.Range(0, _rnd.Next(0, 10)).Select(xxx => new {
                    Prop5 = _rnd.Next(0, 10),
                    Prop6 = $"text-{x}-{xx}-{xxx}",
                }).ToList(),
            }).ToList(),
        }).ToList();

        var vars = new (string[], IEnumerable<object?>)[] {
            (["Even"], items.GroupBy(x => new { x?.Even }).ToList()),
            (["Even","Obj.Type"], items.GroupBy(x => new { x?.Even, x?.Obj?.Type }).ToList()),
            (["Objs"], items.GroupBy(x => new { Objs = x?.Objs?.Count() }).ToList()),
            (["Objs.Count()"], items.GroupBy(x => new { Objs = x?.Objs?.Count() }).ToList()),
            (["Objs.Any()"], items.GroupBy(x => x?.Objs?.Any()).ToList()),
            (["Objs.Sum(Num1)"], items.GroupBy(x => x?.Objs?.Sum(xx => xx?.Num1)).ToList()),
            (["Objs.Sum(Num2)"], items.GroupBy(x => x?.Objs?.Sum(xx => xx?.Num2)).ToList()),
            (["Objs.Sum(Num3)"], items.GroupBy(x => x?.Objs?.Sum(xx => xx?.Num3)).ToList()),
            (["Objs.Sum(Num4)"], items.GroupBy(x => x?.Objs?.Sum(xx => xx?.Num4)).ToList()),
            (["Objs.Sum(Num5)"], items.GroupBy(x => x?.Objs?.Sum(xx => xx?.Num5)).ToList()),
            (["Objs.Sum(Num6)"], items.GroupBy(x => x?.Objs?.Sum(xx => xx?.Num6)).ToList()),
            (["Objs.Sum(Num7)"], items.GroupBy(x => x?.Objs?.Sum(xx => xx?.Num7)).ToList()),
            (["Objs.Sum(Num8)"], items.GroupBy(x => x?.Objs?.Sum(xx => xx?.Num8)).ToList()),
            (["Objs.Sum(Num9)"], items.GroupBy(x => x?.Objs?.Sum(xx => xx?.Num9)).ToList()),
            (["Objs.Average(Num1)"], items.GroupBy(x => x?.Objs?.Any() != true ? null : x?.Objs?.Average(xx => xx?.Num1)).ToList()),
            (["Objs.Average(Num2)"], items.GroupBy(x => x?.Objs?.Any() != true ? null : x?.Objs?.Average(xx => xx?.Num2)).ToList()),
            (["Objs.Average(Num3)"], items.GroupBy(x => x?.Objs?.Any() != true ? null : x?.Objs?.Average(xx => xx?.Num3)).ToList()),
            (["Objs.Average(Num4)"], items.GroupBy(x => x?.Objs?.Any() != true ? null : x?.Objs?.Average(xx => xx?.Num4)).ToList()),
            (["Objs.Average(Num5)"], items.GroupBy(x => x?.Objs?.Any() != true ? null : x?.Objs?.Average(xx => xx?.Num5)).ToList()),
            (["Objs.Average(Num6)"], items.GroupBy(x => x?.Objs?.Any() != true ? null : x?.Objs?.Average(xx => xx?.Num6)).ToList()),
            (["Objs.Average(Num7)"], items.GroupBy(x => x?.Objs?.Any() != true ? null : x?.Objs?.Average(xx => xx?.Num7)).ToList()),
            (["Objs.Average(Num8)"], items.GroupBy(x => x?.Objs?.Any() != true ? null : x?.Objs?.Average(xx => xx?.Num8)).ToList()),
            (["Objs.Average(Num9)"], items.GroupBy(x => x?.Objs?.Any() != true ? null : x?.Objs?.Average(xx => xx?.Num9)).ToList()),
            (["Objs.Min(Date1)"], items.GroupBy(x => x?.Objs?.Any() != true ? null : x?.Objs?.Min(xx => xx?.Date1)).ToList()),
            (["Objs.Max(Date2)"], items.GroupBy(x => x?.Objs?.Any() != true ? null : x?.Objs?.Max(xx => xx?.Date2)).ToList()),
            (["Objs.Min(Obj.Prop4)"], items.GroupBy(x => x?.Objs?.Any() != true ? null : x?.Objs?.Min(xx => xx?.Obj?.Prop4)).ToList()),
            (["Objs.Max(Objs.Sum(Prop5))"], items.GroupBy(x => x?.Objs?.Any() != true ? null : x?.Objs?.Max(xx => xx?.Objs?.Sum(xxx=>xxx.Prop5))).ToList()),
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
    public async Task EfCore1()
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

    [Test]
    public async Task EfCore2()
    {
        using var ctx = new EfCoreContext();

        var query = ctx.Authors
            .Include(x => x.Books);

        var test = await query
            .GroupBy(x => x.Books.Count())
            .ToListAsync();

        var result = await query
            .GroupBy("Books")
            .ToListAsync();

        Assert.That(result, Is.EqualTo(test).AsCollection);

        result = await query
            .GroupBy("Books.Count()")
            .ToListAsync();

        Assert.That(result, Is.EqualTo(test).AsCollection);
    }
#endif
}