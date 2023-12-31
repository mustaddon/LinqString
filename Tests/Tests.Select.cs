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
            }).ToList()
        }).ToList();

        var cache = new NeverExpiredCache();
        var props = new[] { "Prop1", "Prop2.Prop22", "Prop3.Prop32", "Prop3.Count()", "Prop3.Max(Prop31)", "Prop3.Sum(Prop31)" };
        var test = items.Select(x => x == null ? null : new
        {
            x.Prop1,
            Prop2 = x.Prop2 == null ? null : new { x.Prop2.Prop22 },
            Prop3 = x.Prop3?.Select(xx => xx == null ? null : new { xx.Prop32 }),
            Prop3Count = x.Prop3?.Count(),
            Prop3MaxProp31 = x.Prop3?.Max(xx => xx?.Prop31),
            Prop3SumProp31 = x.Prop3?.Sum(xx => xx?.Prop31),
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
}