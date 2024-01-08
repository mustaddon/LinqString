using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Tests;

public class TestSelectLast
{
    [Test]
    public void Last()
    {
        var items = Enumerable.Range(0, 10).Select(x => x == 0 ? null : new
        {
            Prop1 = x,
            Prop3 = x == 2 ? null : Enumerable.Range(0, 5).Select(xx => xx == 0 ? null : new
            {
                Prop31 = xx + x * 1000,
                Prop32 = $"text-{x}-{xx}",
            }).ToList()
        }).ToList();


        var test = items
            .Select(x => x == null ? null : new
            {
                Prop3LastProp32 = x.Prop3?.Select(xx => xx?.Prop32).LastOrDefault(),
            })
            .ToList();

        var queryable = items.AsQueryable()
            .Select("Prop3.Last(Prop32)")
            .ToList();

        Assert.That(JsonSerializer.Serialize(queryable), Is.EqualTo(JsonSerializer.Serialize(test)));

        var enumerable = items
            .Select("Prop3.Last(Prop32)")
            .ToList();

        Assert.That(JsonSerializer.Serialize(enumerable), Is.EqualTo(JsonSerializer.Serialize(test)));
    }

}