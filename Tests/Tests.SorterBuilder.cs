namespace Tests;

public class TestSorterBuilder
{
    [Test]
    public void Build()
    {
        var item = new { Prop1 = 111, Obj = new { Prop2 = "222" } };
        var path = "Obj.Prop2";
        var vars = new[] { "", "<", ">" }.SelectMany(x => new[] { (x, false, x == ">"), (x, true, x != "<") });

        foreach(var (pref, desc, testDesc) in vars)
        {
            var (lambda, finalDesc) = SorterBuilder.Build(item.GetType(), pref + path, desc);

            Assert.That(lambda.ReturnType, Is.EqualTo(item.Obj.Prop2.GetType()));
            Assert.That(finalDesc, Is.EqualTo(testDesc));

            var res = lambda.Compile().DynamicInvoke(item);
            Assert.That(res, Is.EqualTo(item.Obj.Prop2));
        }
    }
}