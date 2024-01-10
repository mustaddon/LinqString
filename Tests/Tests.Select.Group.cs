using System.Text.Json;

namespace Tests;

public class TestSelectGroup
{
    [Test]
    public void Group()
    {
        var items = Enumerable.Range(0, 10).Select(x => x == 0 ? null : new
        {
            Even = x % 2 == 0,
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


        var test = items
            .GroupBy(x => x?.Even)
            .Select(g => g.Select(x => x == null ? null : new
            {
                x.Prop1,
                Prop2 = x.Prop2 == null ? null : new { x.Prop2?.Prop21 },
                Prop3SumProp31 = x.Prop3?.Sum(xx => xx?.Prop31),
            }))
            .ToList();

        var queryable = items.AsQueryable()
            .GroupBy("Even")
            .Select("Prop1", "Prop2.Prop21", "Prop3.Sum(Prop31)")
            .ToList();

        Assert.That(JsonSerializer.Serialize(queryable), Is.EqualTo(JsonSerializer.Serialize(test)));

        var enumerable = items
            .GroupBy("Even")
            .Select("Prop1", "Prop2.Prop21", "Prop3.Sum(Prop31)")
            .ToList();

        Assert.That(JsonSerializer.Serialize(enumerable), Is.EqualTo(JsonSerializer.Serialize(test)));
    }

}