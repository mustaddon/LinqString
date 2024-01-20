using DynamicAnonymousType;
using LinqString._internal;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqString;

public static class SelectorBuilder
{
    public static LambdaExpression Build(Type sourceType, IEnumerable<string> props, bool nullsafeObjects = false, bool nullsafeEnumerables = false)
    {
        return BuildOrdered(sourceType, props.Order(), nullsafeObjects, nullsafeEnumerables);
    }

    internal static LambdaExpression BuildOrdered(Type sourceElementType, IEnumerable<string> orderedProps, bool nullsafeObjects, bool nullsafeEnumerables)
    {
        var ctx = new Ctx
        {
            NullsafeObjects = nullsafeObjects,
            NullsafeEnumerables = nullsafeEnumerables
        };

        var data = FillData(ctx, new(sourceElementType.ToEnumerable()), GetPropTree(orderedProps));
        var param = Expression.Parameter(sourceElementType, null);
        return Expression.Lambda(BindOrInit(ctx, param, data), param);
    }

    static Expression InitExpr(Ctx ctx, Expression source, PropData data)
    {
        var returnType = data.TargetElementType ?? data.TargetType;

        var init = Expression.MemberInit(
            Expression.New(returnType),
            data.Childs!.Select(x => Expression.Bind(returnType.GetProperty(x.Name!)!, BindExpr(ctx, source, x))));

        if (!ctx.NullsafeObjects)
            return init;

        return Expression.Condition(
           source.NotNull(), init,
           Expression.Constant(null, returnType),
           returnType);
    }

    static Expression BindExpr(Ctx ctx, Expression source, PropData data)
    {
        if (data.Member != null)
            source = Expression.MakeMemberAccess(source, data.Member); // Expression.PropertyOrField(source, data.Name!);

        if (data.Method != null)
            return source.PathValue(data.Method!, ctx.NullsafeEnumerables);

        if (!data.IsDynamic)
            return source;

        if (!data.IsEnumerable)
            return InitExpr(ctx, source, data);

        if (!ctx.NullsafeEnumerables)
            return SelectExpr(ctx, source, data);

        return Expression.Condition(source.NotNull(),
            SelectExpr(ctx, source, data),
            Expression.Constant(null, data.TargetType));
    }

    static Expression BindOrInit(Ctx ctx, Expression source, PropData data)
    {
        return data.IsEnumerableEnumerable
            ? BindExpr(ctx, source, data.Childs!.First())
            : InitExpr(ctx, source, data);
    }

    static MethodCallExpression SelectExpr(Ctx ctx, Expression source, PropData data)
    {
        var select = _enumerableSelect.MakeGenericMethod(data.SourceElementType!, data.TargetElementType!);
        var lambdaParam = Expression.Parameter(data.SourceElementType!, null);
        var lambda = Expression.Lambda(BindOrInit(ctx, lambdaParam, data), lambdaParam);
        return Expression.Call(select, source, lambda);
    }

    static readonly MethodInfo _enumerableSelect = new Func<IEnumerable<object>, Func<object, object>, IEnumerable<object>>(Enumerable.Select)
        .Method.GetGenericMethodDefinition();

    static PropData FillData(Ctx ctx, PropData data, PropNode node, bool isFilled = false)
    {
        if (data.SourceType.TryGetElementType(out var sourceType))
        {
            data.SourceElementType = sourceType;
            data.IsEnumerable = true;
        }
        else
        {
            sourceType = data.SourceType;
        }

        var childs = data.Childs = [];

        if (data.IsEnumerable && sourceType.IsEnumerable())
        {
            data.IsEnumerableEnumerable = true;
            childs.Add(FillData(ctx, new(sourceType), node, isFilled));
        }
        else
        {
            PropertyInfo? prop;
            FieldInfo? field;
            PropData? tmp;

            foreach (var childNode in node.Childs!)
            {
                if ((prop = sourceType.GetProperty(childNode.Name)) != null)
                    tmp = new(prop.PropertyType) { Name = childNode.Name, Member = prop };
                else if ((field = sourceType.GetField(childNode.Name)) != null)
                    tmp = new(field.FieldType) { Name = childNode.Name, Member = field };
                else
                    throw new KeyNotFoundException($"'{childNode.Name}' is not a member of type '{sourceType}'");

                if (childNode.IsCustom)
                    FillData(ctx, tmp, childNode, childNode.IsFilled);

                if (childNode.IsFilled || childNode.Childs?.Count > 0)
                    childs.Add(tmp);

                if (childNode.Methods != null)
                    foreach (var method in GetMethods(ctx, tmp, childNode.Methods))
                        childs.Add(method);
            }

            if (isFilled)
            {
                var childNames = new HashSet<string>(childs.Where(x => x.Name != null).Select(x => x.Name!));

                var missed = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.CanRead)
                    .Select(x => new PropData(x.PropertyType) { Name = x.Name, Member = x })
                    .Concat(sourceType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Select(x => new PropData(x.FieldType) { Name = x.Name, Member = x }))
                    .Where(x => !childNames.Contains(x.Name!));

                foreach (var item in missed)
                {
                    childs.Add(item);
                    childNames.Add(item.Name!);
                }
            }
        }

        if (data.IsEnumerableEnumerable)
        {
            var child = childs.First();
            if (child.IsDynamic)
            {
                data.IsDynamic = true;
                data.TargetElementType = child.TargetType;
                data.TargetType = data.TargetElementType.ToEnumerable();
            }
        }
        else if (childs.Count > 0)
        {
            var dynamicType = DynamicFactory.CreateType(childs.Select(x => (x.Name!, x.TargetType!)));

            data.IsDynamic = true;

            if (!data.IsEnumerable)
                data.TargetType = dynamicType;
            else
            {
                data.TargetType = dynamicType.ToEnumerable();
                data.TargetElementType = dynamicType;
            }
        }

        return data;
    }

    static IEnumerable<PropData> GetMethods(Ctx ctx, PropData source, IEnumerable<string> methods)
    {
        var param = Expression.Parameter(source.SourceType, null);

        foreach (var method in methods)
            yield return new(source.SourceType)
            {
                Member = source.Member,
                Method = method,
                Name = string.Concat(source.Name, method.NameFromPath()),
                TargetType = param.PathValue(method, ctx.NullsafeEnumerables).Type,
            };
    }

    static PropNode GetPropTree(IEnumerable<string> orderedPaths)
    {
        var result = new PropNode { Childs = [] };

        foreach (var path in orderedPaths)
        {
            var enumerator = path.SplitPathLight().GetEnumerator();
            var hasNext = enumerator.MoveNext();
            var parent = result;
            var lvl = result.Childs;

            while (hasNext)
            {
                var (name, isFn) = enumerator.Current;
                hasNext = enumerator.MoveNext();

                if (isFn)
                {
                    SetIsCustom(parent.Parent);
                    (parent.Methods ??= []).Add(name);
                    continue;
                }

                var last = lvl.LastOrDefault();

                if (name != last?.Name)
                    lvl.Add(last = new PropNode { Name = name, Parent = parent });

                if (!hasNext)
                    last.IsFilled = true;
                else if (!enumerator.Current.IsFn)
                    SetIsCustom(last);

                if (hasNext)
                {
                    parent = last;
                    lvl = (last.Childs ??= []);
                }
            }
        }

        return result;
    }

    static void SetIsCustom(PropNode? node)
    {
        while (node?.IsCustom == false)
        {
            node.IsCustom = true;
            node = node.Parent;
        }
    }

    class PropNode
    {
        public string Name { get; set; } = default!;
        public bool IsCustom { get; set; }
        public bool IsFilled { get; set; }

        public PropNode? Parent { get; set; }
        public List<PropNode>? Childs { get; set; }
        public List<string>? Methods { get; set; }
    }

    class PropData
    {
        public PropData(Type sourceType)
        {
            TargetType = SourceType = sourceType;
        }

        public bool IsDynamic { get; set; }
        public bool IsEnumerable { get; set; }
        public bool IsEnumerableEnumerable { get; set; }

        public string? Name { get; set; }
        public string? Method { get; set; }
        public MemberInfo? Member { get; set; }
        public Type SourceType { get; }
        public Type? SourceElementType { get; set; }
        public Type TargetType { get; set; }
        public Type? TargetElementType { get; set; }
        public List<PropData>? Childs { get; set; }
    }

    class Ctx
    {
        public bool NullsafeObjects { get; set; }
        public bool NullsafeEnumerables { get; set; }
    }
}
