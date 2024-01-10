using System.Text.Json;

namespace Tests;

public class TestGroupByCascade
{
    [Test]
    public void Cascade()
    {
        var items = Enumerable.Range(0, 10).Select(x => x == 0 ? null : new
        {
            Even = x % 2 == 0,
            Prop1 = x,
            Prop3 = x <= 1 ? null : Enumerable.Range(0, 5)
                .Select(xx => xx <= 2 ? null : Enumerable.Range(0, 6)
                    .Select(xxx => xx <= 3 ? null : Enumerable.Range(0, 7)
                        .Select(xxxx => xxxx <= 4 ? null : new
                        {
                            Prop31 = xxxx + xxx * 100 + xx * 1000 + x * 10000,
                            Prop32 = $"text-{x}-{xx}-{xxx}-{xxxx}",
                        }).ToList()).ToList()).ToList(),
        }).ToList();


        var test = items
            .GroupBy(x => x?.Prop3?.Max(xx => xx?.Max(xxx => xxx?.Max(xxxx => xxxx?.Prop31))))
            .GroupBy(g => g.Max(x=>x?.Even))
            .ToList();

        var queryable = items.AsQueryable()
            .GroupBy("Prop3.Max(Max(Max(Prop31)))")
            .GroupBy("Max(Even)")
            .ToList();

        Assert.That(JsonSerializer.Serialize(queryable), Is.EqualTo(JsonSerializer.Serialize(test)));

        var enumerable = items
            .GroupBy("Prop3.Max(Max(Max(Prop31)))")
            .GroupBy("Max(Even)")
            .ToList();

        Assert.That(JsonSerializer.Serialize(enumerable), Is.EqualTo(JsonSerializer.Serialize(test)));
    }

}