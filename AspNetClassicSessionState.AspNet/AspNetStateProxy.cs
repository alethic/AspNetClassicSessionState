using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Web;

namespace AspNetClassicSessionState.AspNet
{

    /// <summary>
    /// Provides access to get or set the ASP.Net session state items.
    /// </summary>
    public class AspNetStateProxy :
        MarshalByRefObject,
        IStrongBox,
        IDisposable
    {

        readonly WeakReference<HttpContext> context;
        bool disposed = false;

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
        HttpContext GetContext()
        {
            return context.TryGetTarget(out var c) ? c : null;
        }

        /// <summary>
        /// Sets an item in the ASP.Net session.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        void Push(IDictionary<string, object> items)
        {
            var curr = GetContext() ?? throw new InvalidOperationException("Null HttpContext in cache. Cannot reenter ASP.Net request.");
            var prev = HttpContext.Current;

            try
            {
                HttpContext.Current = curr;

                foreach (var item in items)
                {
                    var type = item.Value?.GetType();
                    if (type == null || type.IsPrimitive || type.IsValueType || !Marshal.IsComObject(item.Value))
                        curr.Session[AspNetStateModule.Prefix + item.Key] = item.Value;
                    else
                        AspNetStateModule.Tracer.TraceEvent(TraceEventType.Verbose, 0, "Skipping unsupported ASP classic object type: {0}", item.Value.GetType());
                }
            }
            finally
            {
                HttpContext.Current = prev;
            }
        }

        /// <summary>
        /// Gets the items from the ASP.Net session state.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, object> Pull()
        {
            var curr = GetContext() ?? throw new InvalidOperationException("Null HttpContext in cache. Cannot reenter ASP.Net request.");
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
        /// Used by the remote instance to pull or push values.
        /// </summary>
        public object Value
        {
            get => Pull();
            set => Push((Dictionary<string, object>)value);
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        /// <param name="disposing"></param>
        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    RemotingServices.Disconnect(this);

                    // attempt to remove the reference and allow the context to be free
                    if (GetContext() is HttpContext c)
                        c.Items[AspNetStateModule.ContextProxyItemKey] = null;
                }

                disposed = true;
            }
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Finalizes the object.
        /// </summary>
        ~AspNetStateProxy()
        {
            Dispose(false);
        }

    }

}
