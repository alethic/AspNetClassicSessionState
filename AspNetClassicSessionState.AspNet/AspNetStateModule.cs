using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Web;

using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: WebActivatorEx.PreApplicationStartMethod(
    typeof(AspNetClassicSessionState.AspNet.AspNetStateModule),
    nameof(AspNetClassicSessionState.AspNet.AspNetStateModule.RegisterModule))]

namespace AspNetClassicSessionState.AspNet
{

    /// <summary>
    /// Initializes the application to share state with ASP Classic Requests through the ASP.Net State Server.
    /// </summary>
    public class AspNetStateModule :
        IHttpModule
    {

        public static readonly TraceSource Tracer = new TraceSource("AspNetClassicSessionState");
        public static readonly string Prefix = AspNetClassicStateConfigurationSection.DefaultSection?.Prefix ?? "ASP_";
        public static readonly string ContextProxyPtrKey = "__ASPNETCLASSICPROXY";
        public static readonly string HeadersProxyPtrKey = "ASPNETSTATEPROXYREF";

        /// <summary>
        /// Gets whether or not the ASP Classic session state proxy is enabled.
        /// </summary>
        static bool IsEnabled => AspNetClassicStateConfigurationSection.DefaultSection?.Enabled ?? true;

        /// <summary>
        /// Registers the HTTP module.
        /// </summary>
        public static void RegisterModule()
        {
            DynamicModuleUtility.RegisterModule(typeof(AspNetStateModule));
        }

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        /// <param name="context"></param>
        public void Init(HttpApplication context)
        {
            if (IsEnabled)
            {
                context.AddOnBeginRequestAsync(BeginOnBeginRequestAsync, EndOnBeginRequestAsync);
                context.AddOnMapRequestHandlerAsync(OnBeginMapRequestHandlerAsync, OnEndMapRequestHandlerAsync);
                context.AddOnAcquireRequestStateAsync(OnBeginAcquireRequestStateAsync, OnEndAcquireRequestStateAsync);
                context.AddOnEndRequestAsync(OnBeginEndRequestAsync, OnEndEndRequestAsync);
            }
        }

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
                return new CompletedAsyncResult(true);

            // only generate for classic ASP requests
            if (context.Request.CurrentExecutionFilePathExtension == ".asp")
            {
                // store reference to proxy in context for keep alive
                var proxy = new AspNetStateProxy(HttpContext.Current);
                HttpContext.Current.Items[ContextProxyPtrKey] = proxy;

                // generate CCW for proxy and add to server variables
                var iukwn = Marshal.GetIUnknownForObject(proxy);
                HttpContext.Current.Request.Headers.Add(HeadersProxyPtrKey, iukwn.ToString());
            }

            return new CompletedAsyncResult(true);
        }

        /// <summary>
        /// Invoked when the request is beginning.
        /// </summary>
        /// <param name="ar"></param>
        void EndOnBeginRequestAsync(IAsyncResult ar)
        {

        }

        /// <summary>
        /// Invoked before the handler is mapped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="cb"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        IAsyncResult OnBeginMapRequestHandlerAsync(object sender, EventArgs args, AsyncCallback cb, object extraData)
        {
            // ignore spurious calls
            var context = HttpContext.Current;
            if (context == null)
                return new CompletedAsyncResult(true);

            // register handler to force session state initialization
            if (context.Handler == null)
                context.Handler = new EnableSessionStateHandler();

            return new CompletedAsyncResult(true);
        }

        /// <summary>
        /// Invoked before the handler is mapped.
        /// </summary>
        /// <param name="ar"></param>
        void OnEndMapRequestHandlerAsync(IAsyncResult ar)
        {

        }

        /// <summary>
        /// Invoked before the session state is acquired.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="cb"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        IAsyncResult OnBeginAcquireRequestStateAsync(object sender, EventArgs args, AsyncCallback cb, object extraData)
        {
            // ignore spurious calls
            var context = HttpContext.Current;
            if (context == null)
                return new CompletedAsyncResult(true);

            // unmap our temporary state handler.
            if (context.Handler is EnableSessionStateHandler)
                context.Handler = null;

            return new CompletedAsyncResult(true);
        }

        /// <summary>
        /// Invoked before the session state is acquired.
        /// </summary>
        /// <param name="ar"></param>
        void OnEndAcquireRequestStateAsync(IAsyncResult ar)
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
                return new CompletedAsyncResult(true);

            try
            {
                // last ditch effort to clean up proxy
                if (context.Items.Contains(ContextProxyPtrKey))
                {
                    // attempt to dispose of instance
                    if (context.Items[ContextProxyPtrKey] is IDisposable proxy)
                        proxy.Dispose();

                    // remove reference to instance
                    context.Items[ContextProxyPtrKey] = null;
                }
            }
            catch
            {
                // ignore all exceptions, we tried our best
            }

            try
            {
                if (context.Request.Headers[HeadersProxyPtrKey] is string iunkPtrTxt && long.TryParse(iunkPtrTxt, out var iunkPtrL))
                {
                    // pointer in headers represents a reference to object, release
                    var iunkPtr = new IntPtr(iunkPtrL);
                    if (iunkPtr != IntPtr.Zero)
                        Marshal.Release(iunkPtr);

                    context.Request.Headers[HeadersProxyPtrKey] = null;
                }
            }
            catch
            {
                // ignore all exceptions, we tried our best
            }

            return new CompletedAsyncResult(true);
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
