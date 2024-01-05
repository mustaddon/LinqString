# LinqString [![NuGet version](https://badge.fury.io/nu/LinqString.svg?v105)](http://badge.fury.io/nu/LinqString)
IQueryable.Select by property names (EF compatible extensions)


```C#
using LinqString;


var result = queryableSource
    .OrderBy("SubObj.Prop1", ">Prop2")
    .Select("Prop1", "Prop2", "SubObj.Prop1")
    .ToList();

// is an analogue of

var analogue = queryableSource
    .OrderBy(x => x.SubObj.Prop1)
    .ThenByDescending(x => x.Prop2)
    .Select(x => new { 
        x.Prop1,
        x.Prop2,
        SubObj = new { 
            x.SubObj.Prop1
        },
    }).ToList();
```
[Program.cs](https://github.com/mustaddon/LinqString/tree/main/Examples/Program.cs)
