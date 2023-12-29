using System.Linq.Expressions;
namespace LinqString._internal;

internal class CacheValue
{
    public CacheValue(Func<LambdaExpression> lambdaFactory)
    {
        Lambda = new(lambdaFactory);
        Compiled = new(() => Lambda.Value.Compile());
    }

    public Lazy<LambdaExpression> Lambda { get; set; }
    public Lazy<Delegate> Compiled { get; set; }
}
