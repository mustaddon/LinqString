namespace Tests;

public class TestSorterCacheProvider
{
    [Test]
    public void Cache()
    {
        var cache = new NeverExpiredCache();
        var item = new { Prop1 = 111, Obj = new { Prop2 = "222" } };
        var vars = new[] { "", "<", ">" }.SelectMany(x => new[] { (x, false, x == ">"), (x, true, x != "<") });
        var paths = new (string, object)[] { 
            ("Prop1", item.Prop1), 
            ("Obj", item.Obj), 
            ("Obj.Prop2", item.Obj.Prop2),
        };

        Assert.That(cache.Count, Is.EqualTo(0));

        foreach(var (path, val) in paths)
            foreach (var (pref, desc, testDesc) in vars)
            {
                var (fn, finalDesc) = cache.GetSorterDelegate(item.GetType(), pref + path, desc);
                var res = fn.DynamicInvoke(item);
                Assert.That(res, Is.EqualTo(val));
            }

        Assert.That(cache.Count, Is.EqualTo(paths.Length));
    }
}