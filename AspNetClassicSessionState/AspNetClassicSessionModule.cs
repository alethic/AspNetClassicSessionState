using System;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.SessionState;

using Cogito.Collections;
using Cogito.Threading;

using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: WebActivatorEx.PreApplicationStartMethod(
    typeof(AspNetClassicSessionState.AspNetClassicSessionModule),
    nameof(AspNetClassicSessionState.AspNetClassicSessionModule.RegisterModule))]

namespace AspNetClassicSessionState
{

    /// <summary>
    /// Initializes the application to share state with ASP Classic Requests through the ASP.Net State system.
    /// </summary>
    public class AspNetClassicSessionModule :
        IHttpModule
    {

        public static readonly string SessionProxyKey = "__ASPNETCLASSICSESSIONPROXY";
        public static readonly string HeaderProxyPtrKey = "ASPNETCLASSICSESSIONPROXY";
        public static readonly string DisposerActionKey = "ASPNETCLASSICSESSIONDISPOSER";
        static readonly AspNetClassicSessionConfigurationSection config = AspNetClassicSessionConfigurationSection.DefaultSection;

        /// <summary>
        /// Registers the HTTP module.
        /// </summary>
        public static void RegisterModule()
        {
            DynamicModuleUtility.RegisterModule(typeof(AspNetClassicSessionModule));
        }

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        /// <param name="context"></param>
        public void Init(HttpApplication context)
        {
            if (config.Enabled)
            {
                context.AddOnBeginRequestAsync(BeginOnBeginRequestAsync, EndOnBeginRequestAsync);
                context.AddOnEndRequestAsync(OnBeginEndRequestAsync, OnEndEndRequestAsync);
            }
        }

        /// <summary>
        /// Returns true if the given page is an ASP page.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool IsAspPage(HttpContext context) =>
            string.Equals(context.Request.CurrentExecutionFilePathExtension, ".asp", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Invoked when the request is beginning.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="cb"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        IAsyncResult BeginOnBeginRequestAsync(object sender, EventArgs args, AsyncCallback cb, object extraData)
        {
            // ignore spurious calls
            var context = HttpContext.Current;
            if (context == null)
                return new CompletedAsyncResult(null, null);

            // only generate for classic ASP requests
            if (IsAspPage(context))
            {
                // session state is always required
                context.SetSessionStateBehavior(SessionStateBehavior.Required);

                // store reference to proxy in context for keep alive
                var proxy = new AspNetSessionProxy(context, config);
                context.Items[SessionProxyKey] = proxy;

                // generate CCW for proxy and add to server variables
                var intPtr = Marshal.GetIUnknownForObject(proxy);
                context.Request.Headers.Add(HeaderProxyPtrKey, intPtr.ToInt64().ToString("X"));

                // ensures that the reference is released if the context is abandoned
                context.Items[DisposerActionKey] = new DisposableAction(() =>
                {
                    while (Marshal.Release(intPtr) > 0)
                        continue;
                });
            }

            return new CompletedAsyncResult(null, null);
        }

        /// <summary>
        /// Invoked when the request is beginning.
        /// </summary>
        /// <param name="ar"></param>
        void EndOnBeginRequestAsync(IAsyncResult ar)
        {

        }

        /// <summary>
        /// Invoked after the request.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="cb"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        IAsyncResult OnBeginEndRequestAsync(object sender, EventArgs args, AsyncCallback cb, object extraData)
        {
            // ignore spurious calls
            var context = HttpContext.Current;
            if (context == null)
                return new CompletedAsyncResult(null, null);

            // only generate for classic ASP requests
            if (IsAspPage(context))
            {
                // execute disposer
                if (context.Items.GetOrDefault(DisposerActionKey) is IDisposable disposer)
                    disposer.Dispose();

                // remove items from context of possible
                context.Items.Remove(SessionProxyKey);
                context.Items.Remove(DisposerActionKey);
            }

            return new CompletedAsyncResult(null, null);
        }

        /// <summary>
        /// Invoked after the request.
        /// </summary>
        /// <param name="ar"></param>
        void OnEndEndRequestAsync(IAsyncResult ar)
        {

        }

        public void Dispose()
        {

        }

    }

}
