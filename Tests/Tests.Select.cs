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

        var cacheless = items.AsQueryable().Select(props).ToList();
        var cachable = items.AsQueryable().Select(props, cache).ToList();

        Assert.That(JsonSerializer.Serialize(cacheless), Is.EqualTo(JsonSerializer.Serialize(test)));
        Assert.That(JsonSerializer.Serialize(cachable), Is.EqualTo(JsonSerializer.Serialize(test)));

        cacheless = items.Select(props).ToList();
        cachable = items.Select(props, cache).ToList();

        Assert.That(JsonSerializer.Serialize(cacheless), Is.EqualTo(JsonSerializer.Serialize(test)));
        Assert.That(JsonSerializer.Serialize(cachable), Is.EqualTo(JsonSerializer.Serialize(test)));
    }
}