using System;
using System.Buffers;

namespace Positron.Utility
{
    public sealed class PooledDynamicArraySegment<T> : IDisposable
    {
        private T[] _array;
        private int _count;

        public int Count => _count;
        public bool Disposed { get; private set; }

        public T this[int index]
        {
            get 
            { 
                return _array[index];
            }

            set 
            {
                _array[index] = value;
            }
        }

        public PooledDynamicArraySegment(int capacity = 64)
        {
            _array = ArrayPool<T>.Shared.Rent(capacity);
        }

        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(_array);
            Disposed = true;
        }

        public void Add(T item)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException($"Can`t use {GetType().Name} T<{typeof(T)}> after disposing it ");
            }

            CheckAndResize();

            _array[_count] = item;
            _count++;
        }

        public void RemoveAndDisorder(int indexAt)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException($"Can`t use {GetType().Name} T<{typeof(T)}> after disposing it ");
            }

            _count--;
            _array[indexAt] = _array[_count];
            _array[_count] = default;
        }

        public void Clear()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException($"Can`t use {GetType().Name} T<{typeof(T)}> after disposing it ");
            }

            _array.AsSpan(0, _count).Clear();
            _count = 0;
        }

        public ArraySegment<T> ToArray() 
        {
            if (Disposed)
            {
                throw new ObjectDisposedException($"Can`t use {GetType().Name} T<{typeof(T)}> after disposing it ");
            }

            return new ArraySegment<T>(_array, 0, _count);
        }

        private void CheckAndResize()
        {
            if (_count + 1 >= _array.Length)
            {
                T[] biggerArray = ArrayPool<T>.Shared.Rent(_count + 64);
                Array.Copy(_array, biggerArray, _count);
                ArrayPool<T>.Shared.Return(_array);
                _array = biggerArray;
            }
        }
    }
}