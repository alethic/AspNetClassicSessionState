using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Web;

namespace AspNetClassicSessionState.AspNet
{

    /// <summary>
    /// Provides access to get or set the ASP.Net session state items.
    /// </summary>
    public class AspNetStateProxy :
        MarshalByRefObject,
        IStrongBox
    {

        static readonly HttpContextCache cache = new HttpContextCache();

        /// <summary>
        /// Gets the cached <see cref="HttpContext"/> by key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="context"></param>
        public static void SetContext(string key, HttpContext context) => cache.Set(key, context);

        /// <summary>
        /// Sets the cached <see cref="HttpContext"/> by key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static HttpContext GetContext(string key) => cache.Get(key);

        /// <summary>
        /// Describes the prefix to insert before ASP.Net classic session state keys.
        /// </summary>
        public static string Prefix => ((AspNetClassicStateConfigurationSection)ConfigurationManager.GetSection("aspNetClassicSessionState"))?.Prefix ?? "ASP_";


        /// <summary>
        /// Key of the current proxy.
        /// </summary>
        readonly string key;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="key"></param>
        public AspNetStateProxy(string key)
        {
            this.key = key ?? throw new ArgumentNullException(nameof(key));
        }

        /// <summary>
        /// Sets an item in the ASP.Net session.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        void Push(IDictionary<string, object> items)
        {
            var c = GetContext(key);
            if (c == null)
                throw new InvalidOperationException("Null HttpContext in cache. Cannot reenter ASP.Net request.");

            var p = HttpContext.Current; 

            try
            {
                HttpContext.Current = c;

                foreach (var kvp in items)
                    c.Session[Prefix + kvp.Key] = kvp.Value;
            }
            finally
            {
                HttpContext.Current = p;
            }
        }

        /// <summary>
        /// Gets the items from the ASP.Net session state.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, object> Pull()
        {
            var c = GetContext(key);
            if (c == null)
                throw new InvalidOperationException("Null HttpContext in cache. Cannot reenter ASP.Net request.");

            var p = HttpContext.Current;

            try
            {
                HttpContext.Current = c;

                var d = new Dictionary<string, object>();
                foreach (string key in c.Session)
                    if (key.StartsWith(Prefix))
                        d[key.Substring(Prefix.Length)] = c.Session[key];
                return d;
            }
            finally
            {
                HttpContext.Current = p;
            }

        }

        /// <summary>
        /// Used by the remote instance to pull or push values.
        /// </summary>
        public object Value
        {
            get => Pull();
            set => Push((Dictionary<string, object>)value);
        }

    }

}
