using LinqString;
using System.Text.Json;


var items = Enumerable.Range(0, 10).Select(x => new
{
    Even = x % 2 == 0,
    Prop1 = x,
    Prop2 = new
    {
        Prop21 = x,
        Prop22 = $"text-{x}",
    },
    Prop3 = Enumerable.Range(0, 3).Select(xx => new
    {
        Prop31 = xx,
        Prop32 = $"text-{x}-{xx}",
    })
}).AsQueryable();


var result = items
    .OrderBy("Even", ">Prop2.Prop21")
    .Select("Prop1", "Prop2.Prop22", "Prop3.Prop32")
    .ToList();

Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));