﻿using LinqString._internal;
using DynamicAnonymousType;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqString;

public static class SelectorBuilder
{
    public static LambdaExpression Build(Type sourceType, IEnumerable<string> props, bool nullsafeObjects, bool nullsafeEnumerables)
    {
        return BuildOrdered(sourceType, props.Order(), nullsafeObjects, nullsafeEnumerables);
    }

    internal static LambdaExpression BuildOrdered(Type sourceType, IEnumerable<string> orderedProps, bool nullsafeObjects, bool nullsafeEnumerables)
    {
        var data = FillData(new(sourceType), GetPropNodes(orderedProps));
        var param = Expression.Parameter(sourceType, null);
        var body = MemberInitExpr(new Ctx
        {
            NullsafeObjects = nullsafeObjects,
            NullsafeEnumerables = nullsafeEnumerables
        }, param, data);
        return Expression.Lambda(body, param);
    }

    static Expression MemberInitExpr(Ctx ctx, Expression source, PropData data)
    {
        var returnType = data.TargetElementType ?? data.TargetType;

        var init = Expression.MemberInit(
            Expression.New(returnType),
            data.Childs!.Select(x => Expression.Bind(returnType.GetProperty(x.Name!)!, BindingExpr(ctx, source, x))));

        if (!ctx.NullsafeObjects)
            return init;

        return Expression.Condition(
           source.NotNull(), init,
           Expression.Constant(null, returnType),
           returnType);
    }

    static MemberExpression MemberAccessExpr(Expression source, PropData data)
    {
        return Expression.MakeMemberAccess(source, data.Member!);
        //return Expression.PropertyOrField(source, data.Name!);
    }

    static Expression BindingExpr(Ctx ctx, Expression source, PropData data)
    {
        var memberAccess = MemberAccessExpr(source, data);

        if (!data.IsDynamic)
            return memberAccess;

        if (!data.IsEnumerable)
            return MemberInitExpr(ctx, memberAccess, data);

        if (!ctx.NullsafeEnumerables)
            return SelectExpr(ctx, memberAccess, data);

        var returnType = data.TargetType;

        return Expression.Condition(
            memberAccess.NotNull(), SelectExpr(ctx, memberAccess, data),
            Expression.Constant(null, returnType),
            returnType);
    }

    static MethodCallExpression SelectExpr(Ctx ctx, Expression source, PropData data)
    {
        var lambdaParam = Expression.Parameter(data.SourceElementType!, null);
        var lambda = Expression.Lambda(MemberInitExpr(ctx, lambdaParam, data), lambdaParam);
        var select = _enumerableSelect.MakeGenericMethod(data.SourceElementType!, data.TargetElementType!);
        return Expression.Call(select, source, lambda);
    }

    static readonly MethodInfo _enumerableSelect = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
        .FirstOrDefault(x => x.Name == nameof(Enumerable.Select) && x.HasIEnumerableAndFunc())!;

    static PropData FillData(PropData data, IEnumerable<PropNode> orderedNodes, bool isFilled = false)
    {
        data.SourceElementType = data.SourceType == typeof(string) ? null : data.SourceType.GetElementTypeExt();
        data.IsEnumerable = data.SourceElementType != null;

        var childs = data.Childs = new();
        var childNames = new HashSet<string>();
        var sourceType = data.SourceElementType ?? data.SourceType;

        PropertyInfo? prop;
        FieldInfo? field;
        PropData? tmp;

        foreach (var node in orderedNodes)
        {
            if ((prop = sourceType.GetProperty(node.Name)) != null)
                tmp = new(prop.PropertyType) { Name = node.Name, Member = prop };
            else if ((field = sourceType.GetField(node.Name)) != null)
                tmp = new(field.FieldType) { Name = node.Name, Member = field };
            else
                continue;

            if (node.IsCustom)
                FillData(tmp, node.Childs!, node.IsFilled);

            childs.Add(tmp);
            childNames.Add(tmp.Name);
        }

        if (isFilled)
        {
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


        if (childs.Count > 0)
        {
            var dynamicType = DynamicFactory.CreateType(childs.Select(x => (x.Name!, x.TargetType!)));

            data.IsDynamic = true;

            if (!data.IsEnumerable)
                data.TargetType = dynamicType;
            else
            {
                data.TargetType = typeof(IEnumerable<>).MakeGenericType(dynamicType);
                data.TargetElementType = dynamicType;
            }
        }

        return data;
    }

    static List<PropNode> GetPropNodes(IEnumerable<string> orderedPaths)
    {
        var result = new List<PropNode>();

        foreach (var path in orderedPaths)
        {
            var props = path.Split('.');

            var lvl = result;
            PropNode? parent = null;

            for (var i = 0; i < props.Length; i++)
            {
                var name = props[i];
                var last = lvl.LastOrDefault();
                var hasNext = i + 1 < props.Length;

                if (name != last?.Name)
                {
                    lvl.Add(last = new PropNode { Name = name, Parent = parent });

                    if (!hasNext)
                        last.IsFilled = true;
                    else
                    {
                        last.IsCustom = true;
                        var cur = last.Parent;
                        while (cur?.IsCustom == false)
                        {
                            cur.IsCustom = true;
                            cur = cur.Parent;
                        }
                    }
                }

                if (hasNext)
                {
                    parent = last;
                    lvl = (last.Childs ??= new());
                }
            }
        }

        return result;
    }

    class PropNode
    {
        public string Name { get; set; } = default!;
        public bool IsCustom { get; set; }
        public bool IsFilled { get; set; }

        public PropNode? Parent { get; set; }
        public List<PropNode>? Childs { get; set; }
    }

    class PropData
    {
        public PropData(Type sourceType)
        {
            TargetType = SourceType = sourceType;
        }

        public bool IsDynamic { get; set; }
        public bool IsEnumerable { get; set; }

        public string? Name { get; set; }
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
