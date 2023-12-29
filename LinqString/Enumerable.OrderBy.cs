﻿using LinqString._internal;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace LinqString;

public static class EnumerableOrderExtensions
{
    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, params string[] props)
        => OrderBy(source, props.AsEnumerable());
    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, IEnumerable<string> props)
        => Order(source, props, false, Builder) ?? source.Order();
    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, IEnumerable<string> props, IMemoryCache cache, Action<ICacheEntry>? options = null)
        => Order(source, props, false, CacheProvider(cache, options)) ?? source.Order();


    public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, params string[] props)
        => OrderByDescending(source, props.AsEnumerable());
    public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, IEnumerable<string> props)
        => Order(source, props, true, Builder) ?? source.OrderDescending();
    public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, IEnumerable<string> props, IMemoryCache cache, Action<ICacheEntry>? options = null)
        => Order(source, props, true, CacheProvider(cache, options)) ?? source.OrderDescending();


    public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, params string[] props)
        => ThenBy(source, props.AsEnumerable());
    public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, IEnumerable<string> props)
        => Then(source, props.GetEnumerator(), false, Builder);
    public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, IEnumerable<string> props, IMemoryCache cache, Action<ICacheEntry>? options = null)
        => Then(source, props.GetEnumerator(), false, CacheProvider(cache, options));


    public static IOrderedEnumerable<T> ThenByDescending<T>(this IOrderedEnumerable<T> source, params string[] props)
        => ThenByDescending(source, props.AsEnumerable());
    public static IOrderedEnumerable<T> ThenByDescending<T>(this IOrderedEnumerable<T> source, IEnumerable<string> props)
        => Then(source, props.GetEnumerator(), false, Builder);
    public static IOrderedEnumerable<T> ThenByDescending<T>(this IOrderedEnumerable<T> source, IEnumerable<string> props, IMemoryCache cache, Action<ICacheEntry>? options = null)
        => Then(source, props.GetEnumerator(), false, CacheProvider(cache, options));


    private static IOrderedEnumerable<T>? Order<T>(IEnumerable<T> source, IEnumerable<string> props, bool defaultDesc, SorterFactory sorterFactory)
    {
        var enumerator = props.GetEnumerator();

        if (!enumerator.MoveNext())
            return null;

        var type = typeof(T);
        var (sorter, desc) = sorterFactory(type, enumerator.Current, defaultDesc);
        var method = (desc ? _orderByDesc : _orderBy)!.MakeGenericMethod(type, sorter.Method.ReturnType);

        return Then((IOrderedEnumerable<T>)method.Invoke(null, [source, sorter])!, enumerator, defaultDesc, sorterFactory);
    }

    private static IOrderedEnumerable<T> Then<T>(IOrderedEnumerable<T> source, IEnumerator<string> enumerator, bool defaultDesc, SorterFactory sorterFactory)
    {
        var type = typeof(T);
        while (enumerator.MoveNext())
        {
            var (sorter, desc) = sorterFactory(type, enumerator.Current, defaultDesc);
            var method = (desc ? _thenByDesc : _thenBy)!.MakeGenericMethod(type, sorter.Method.ReturnType);

            source = (IOrderedEnumerable<T>)method.Invoke(null, [source, sorter])!;
        }
        return source;
    }

    private static (Delegate Fn, bool Desc) Builder(Type type, string path, bool desc)
    {
        var (lambda, finalDesc) = SorterBuilder.Build(type, path, desc);
        return (lambda.Compile(), finalDesc);
    }

    private static SorterFactory CacheProvider(IMemoryCache cache, Action<ICacheEntry>? options)
    {
        return (Type type, string path, bool desc) => cache.GetSorterDelegate(type, path, desc, options);
    }


    delegate (Delegate Fn, bool Desc) SorterFactory(Type type, string path, bool desc);

    static readonly MethodInfo? _orderBy;
    static readonly MethodInfo? _orderByDesc;
    static readonly MethodInfo? _thenBy;
    static readonly MethodInfo? _thenByDesc;

    static EnumerableOrderExtensions()
    {
        foreach (var x in typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            if (x.IsGenericMethod && x.HasIEnumerableAndFunc())
                switch (x.Name)
                {
                    case nameof(Enumerable.OrderBy): _orderBy = x; break;
                    case nameof(Enumerable.OrderByDescending): _orderByDesc = x; break;
                    case nameof(Enumerable.ThenBy): _thenBy = x; break;
                    case nameof(Enumerable.ThenByDescending): _thenByDesc = x; break;
                };
        }
    }
}
