using System.Collections;
namespace LinqString._internal;

internal static partial class EnumerableExt
{
    public static IReadOnlyCollection<T> Buffer<T>(this IEnumerable<T> source) => new Bufferable<T>(source);

    private class Bufferable<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        public Bufferable(IEnumerable<T> source)
        {
            if (source is ICollection<T> sourceCol && sourceCol.IsReadOnly)
                _buffer = sourceCol;
            else
                _source = source;
        }

        private IEnumerable<T>? _source;
        private ICollection<T>? _buffer;

        public bool IsReadOnly => true;

        public int Count
        {
            get
            {
                if (_source == null)
                    return _buffer!.Count;

                var result = 0;

                foreach (var x in this)
                    result++;

                return result;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_source == null)
            {
                foreach (var x in _buffer!)
                    yield return x;
            }
            else
            {
                if (_buffer == null)
                    _buffer = new List<T>();
                else
                    _buffer.Clear();

                foreach (var x in _source!)
                {
                    yield return x;
                    _buffer.Add(x);
                }
                _source = null;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void Add(T item) => throw new NotSupportedException();
        public bool Remove(T item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (_source == null || Count > 0) 
                _buffer!.CopyTo(array, arrayIndex);
        }

        public bool Contains(T item)
        {
            if (_source == null)
                return _buffer!.Contains(item);

            var result = false;

            foreach (var x in this)
                if (!result && object.Equals(x, item))
                    result = true;

            return result;
        }

        

    }
}

