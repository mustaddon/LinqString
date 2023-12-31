﻿using LinqString;


var someQeryableSource = Enumerable.Range(0, 10).Select(x => new
{
    Even = x % 2 == 0,
    Prop1 = x,
    Prop2 = new
    {
        Prop21 = x,
        Prop22 = $"text-{x}",
    },
    Prop3 = Enumerable.Range(0, x).Select(xx => new
    {
        Prop31 = xx,
        Prop32 = $"text-{x}-{xx}",
        Prop33 = DateTime.Today.AddDays(xx),
    }),
}).AsQueryable();

var result = someQeryableSource
    .OrderBy("Even", ">Prop3.Sum(Prop31)")
    .Select("Prop1", "Prop2.Prop22", "Prop3.Max(Prop33)")
    .ToList();

Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));



// Dynamic expressions caching (OPTIONAL)

var someServiceProvider = new ServiceCollection().AddMemoryCache().BuildServiceProvider();

var withSlidingCache = someQeryableSource
    .Select(["Prop1", "Prop2.Prop22", "Prop3.Prop32"],
        someServiceProvider.GetRequiredService<IMemoryCache>(),
        o => o.SetSlidingExpiration(TimeSpan.FromSeconds(30)))
    .ToList();

var withNeverExpiredCache = someQeryableSource
    .Select(["Prop1", "Prop2.Prop22", "Prop3.Prop32"], NeverExpiredCache.Instance)
    .ToList();



// OR configure once for all queries

DefaultCacheSettings.Instance = someServiceProvider.GetRequiredService<IMemoryCache>();
DefaultCacheSettings.Entry = o => o.SetSlidingExpiration(TimeSpan.FromSeconds(30));

var withDefaultCache = someQeryableSource
    .Select("Prop1", "Prop2.Prop22", "Prop3.Prop32")
    .ToList();