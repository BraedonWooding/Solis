using System;
using System.Reflection.Metadata;
using System.Threading;

namespace SolisCore.Transpilers
{
    /// <summary>
    /// This exists in System.Reflection.Metadata but as an internal class for some reason
    /// </summary>
    internal sealed class ObjectPool<T> where T : class
    {
        private struct Element
        {
            internal T Value;
        }

        private readonly Element[] _items;

        private readonly Func<T> _factory;

        internal ObjectPool(Func<T> factory, int size)
        {
            _factory = factory;
            _items = new Element[size];
        }

        private T CreateInstance()
        {
            return _factory();
        }

        internal T Allocate()
        {
            Element[] items = _items;
            int num = 0;
            T val;
            while (true)
            {
                if (num < items.Length)
                {
                    val = items[num].Value;
                    if (val != null && val == Interlocked.CompareExchange(ref items[num].Value!, null, val))
                    {
                        break;
                    }

                    num++;
                    continue;
                }

                val = CreateInstance();
                break;
            }

            return val;
        }

        internal void Free(T obj)
        {
            Element[] items = _items;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Value == null)
                {
                    items[i].Value = obj;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// This exists in System.Reflection.Metadata but as an internal class for some reason
    /// </summary>
    internal sealed class PooledBlobBuilder : BlobBuilder
    {
        private const int PoolSize = 128;

        private const int ChunkSize = 1024;

        private static readonly ObjectPool<PooledBlobBuilder> s_chunkPool = new(() => new PooledBlobBuilder(ChunkSize), PoolSize);

        private PooledBlobBuilder(int size)
            : base(size)
        {
        }

        public static PooledBlobBuilder GetInstance()
        {
            return s_chunkPool.Allocate();
        }

        protected override BlobBuilder AllocateChunk(int minimalSize)
        {
            if (minimalSize <= ChunkSize)
            {
                return s_chunkPool.Allocate();
            }

            return new BlobBuilder(minimalSize);
        }

        protected override void FreeChunk()
        {
            s_chunkPool.Free(this);
        }
        public new void Free()
        {
            base.Free();
        }
    }
}
