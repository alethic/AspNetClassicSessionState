using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace AspNetClassicSessionState.AspNet
{

    /// <summary>
    /// Provides access to get or set the ASP.Net session state items.
    /// </summary>
    [Guid("E3A70CB0-FA23-4123-8BE3-01F85343441F")]
    [ComVisible(true)]
    public class AspNetStateProxy
    {

        readonly WeakReference<HttpContext> context;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public AspNetStateProxy(HttpContext context)
        {
            this.context = new WeakReference<HttpContext>(context);
        }

        /// <summary>
        /// Attempts to get the current context.
        /// </summary>
        /// <returns></returns>
        HttpContext Context => context.TryGetTarget(out var c) ? c : null;

        /// <summary>
        /// Gets the items from the ASP.Net session state.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> Load()
        {
            var curr = Context ?? throw new InvalidOperationException("Null HttpContext in cache. Cannot reenter ASP.Net request.");
            var prev = HttpContext.Current;

            try
            {
                HttpContext.Current = curr;

                var dict = new Dictionary<string, object>();
                foreach (string key in curr.Session)
                    if (key.StartsWith(AspNetStateModule.Prefix))
                        dict[key.Substring(AspNetStateModule.Prefix.Length)] = curr.Session[key];

                return dict;
            }
            finally
            {
                HttpContext.Current = prev;
            }

        }

        /// <summary>
        /// Sets an item in the ASP.Net session.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void Save(Dictionary<string, object> items)
        {
            var cntx = Context ?? throw new InvalidOperationException("Null HttpContext in cache. Cannot reenter ASP.Net request.");
            var prev = HttpContext.Current;

            try
            {
                HttpContext.Current = cntx;

                // copy ASP session items to ASP.Net session
                foreach (var item in items)
                {
                    var type = item.Value?.GetType();
                    if (type == null || type.IsPrimitive || type.IsValueType || !Marshal.IsComObject(item.Value))
                        cntx.Session[AspNetStateModule.Prefix + item.Key] = item.Value;
                    else
                        AspNetStateModule.Tracer.TraceEvent(TraceEventType.Verbose, 0, "Skipping unsupported ASP classic object type: {0}", item.Value.GetType());
                }

                // remove ASP session items that are no longer within collection
                foreach (var key in cntx.Session.Keys.Cast<string>().ToList())
                    if (key.StartsWith(AspNetStateModule.Prefix) && items.ContainsKey(key.Substring(AspNetStateModule.Prefix.Length)) == false)
                        cntx.Session.Remove(key);
            }
            finally
            {
                HttpContext.Current = prev;
            }
        }

    }

}
