using System;
using System.Collections.Concurrent;
using System.Web;

namespace AspNetClassicSessionState.AspNet
{

    class HttpContextCache
    {

        readonly ConcurrentDictionary<string, WeakReference<HttpContext>> t;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public HttpContextCache()
        {
            t = new ConcurrentDictionary<string, WeakReference<HttpContext>>();
        }

        /// <summary>
        /// Gets the <see cref="HttpContext"/> from the cache;
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public HttpContext Get(string key)
        {
            if (t.TryGetValue(key, out var v) &&
                v.TryGetTarget(out var c))
                return c;
            else
                return null;
        }

        /// <summary>
        /// Adds the <see cref="HttpContext"/> to the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="context"></param>
        public void Set(string key, HttpContext context)
        {
            t.AddOrUpdate(key, k => new WeakReference<HttpContext>(context), (k, v) => v);
        }

    }

}
