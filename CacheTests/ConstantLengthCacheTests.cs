using ConstantLengthCache;
using NUnit.Framework;
using System.Collections.Generic;

namespace CacheTests
{
    public class ConstantLengthCacheTests
    {
        [SetUp]
        public void Setup()
        {
        }

        public ConstantLengthCache<string> GetCache()
        {
            Dictionary<long, string> remoteStorage = new Dictionary<long, string>()
            {
                {1, "==1==" },
                {2, "==2==" },
                {3, "==3==" },
                {4, "==4==" },
                {5, "==5==" },
                {6, "==6==" },
                {7, "==7==" },
                {8, "==8==" },
                {9, "==9==" },
            };

            ConstantLengthCache<string> cache = new ConstantLengthCache<string>(3, 6,
                (id) => remoteStorage[id],
                (id, str) => remoteStorage[id] = str);

            return cache;
        }

        [Test]
        public void Overflow_Test()
        {
            var cache = GetCache();

            Assert.AreEqual(cache.Count, 0);

            cache.Get(3);
            cache.Get(4);
            cache.Get(5);
            cache.Get(6);
            cache.Get(7);
            cache.Get(8);
            Assert.AreEqual(cache.Count, 6);

            cache.Get(9);
            Assert.AreEqual(cache.Count, 3);

            Assert.IsTrue(cache.ExistInCache(9));
            Assert.IsTrue(cache.ExistInCache(8));
            Assert.IsTrue(cache.ExistInCache(7));

            Assert.IsFalse(cache.ExistInCache(6));
            Assert.IsFalse(cache.ExistInCache(5));
            Assert.IsFalse(cache.ExistInCache(4));
            Assert.IsFalse(cache.ExistInCache(3));
        }

        [Test]
        public void Get_Test()
        {
            var cache = GetCache();

            string s3 = cache.Get(3);
            Assert.AreEqual(s3, "==3==");
        }

        [Test]
        public void Add_Test()
        {
            var cache = GetCache();

            cache.Add(3, "temp");

            var s3 = cache.Get(3);
            Assert.AreEqual(s3, "temp");
        }

        [Test]
        public void Clear_Test()
        {
            var cache = GetCache();
            cache.Add(3, "temp");
            cache.Clear();
            Assert.AreEqual(cache.Count, 0);
            Assert.AreEqual(cache.Get(3), "temp");
        }
    }
}