using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Tests;

public class TestSelectÑascade
{
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


}