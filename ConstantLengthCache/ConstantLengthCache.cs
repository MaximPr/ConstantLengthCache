using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstantLengthCache
{
    public class ConstantLengthCache<T>
    {
        private class CacheData<K>
        {
            public K data;
            public long key;
            public long changeTick;
        }

        private long changeTicks = 0;
        private readonly int maxSize = 1000;
        private readonly int recommendedSize = 500;
        private readonly Func<long, T> rememberFunction;
        private readonly Action<long, T> forgetFunction;
        private object cache_lock = new object();
        private Dictionary<long, CacheData<T>> cache = new Dictionary<long, CacheData<T>>();

        public ConstantLengthCache(int recommendedSize, int maxSize, Func<long, T> rememberFunction, Action<long, T> forgetFunction)
        {
            this.maxSize = maxSize;
            this.recommendedSize = recommendedSize;
            this.rememberFunction = rememberFunction;
            this.forgetFunction = forgetFunction;
        } 

        public void ForgetExcess()
        {
            List<CacheData<T>> list;
            lock (cache_lock)
            {
                list = cache.Values.ToList();
            }
            list.Sort((x, y) => (int)(y.changeTick - x.changeTick));
            for (int i = recommendedSize; i < list.Count; i++)
            {
                var cacheData = list[i];
                lock (cache_lock)
                {
                    cache.Remove(cacheData.key);
                }
                forgetFunction(cacheData.key, cacheData.data);
            }
        }

        public void Clear()
        {
            lock (cache_lock)
            {
                foreach(var cacheData in cache.Values)
                {
                    forgetFunction(cacheData.key, cacheData.data);
                }
                cache.Clear();
            }
        }

        public int Count
        {
            get
            {
                lock (cache_lock)
                {
                    return cache.Count;
                }
            }
        }

        public bool ExistInCache(long key)
        {
            lock (cache_lock)
            {
                return cache.ContainsKey(key);
            }
        }

        public IEnumerable<T> AllCachedValues()
        {
            lock (cache_lock)
            {
                return cache.Values.Select(x => x.data);
            }
        }

        private T Remember(long key)
        {
            T data = rememberFunction(key);
            Add(key, data);
            return data;
        }

        public void Add(long key, T data)
        {
            var cacheData = new CacheData<T>() { key = key, data = data, changeTick = changeTicks++ };
            lock (cache_lock)
            {
                cache[key] = cacheData;
            }

            if (Count > maxSize)
                ForgetExcess();
        }

        public T Get(long key)
        {
            T data;
            CacheData<T> cacheData;
            lock (cache_lock)
            {
                cache.TryGetValue(key, out cacheData);
            }
            if (cacheData == null)
                data = Remember(key);
            else
            {
                cacheData.changeTick = changeTicks++;
                data = cacheData.data;
            }

            return data;
        }
    }
}
