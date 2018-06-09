using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters.Binary;
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

        public readonly static TraceSource Tracer = new TraceSource("AspNetClassicSessionState");
        public readonly static string Prefix = AspNetClassicStateConfigurationSection.DefaultSection?.Prefix ?? "ASP_";
        public readonly static string ContextProxyItemKey = "__ASPNETCLASSICPROXY";

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
        /// Serializes the ObjRef to a base64 encoded string.
        /// </summary>
        /// <param name="objRef"></param>
        /// <returns></returns>
        string SerializeObjRef(ObjRef objRef)
        {
            using (var stm = new MemoryStream())
            using (var cmp = new DeflateStream(stm, CompressionMode.Compress, true))
            {
                var srs = new BinaryFormatter();
                srs.Serialize(cmp, objRef);
                cmp.Flush();
                cmp.Dispose();
                stm.Position = 0;
                return Convert.ToBase64String(stm.ToArray());
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
            // generate a new proxy and serialize an objref to that proxy into the request headers
            var proxy = new AspNetStateProxy(HttpContext.Current);
            HttpContext.Current.Items[ContextProxyItemKey] = proxy;
            var objRef = RemotingServices.Marshal(proxy, null, typeof(IStrongBox));
            HttpContext.Current.Request.Headers.Add("ASPNETSTATEPROXYREF", SerializeObjRef(objRef));

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
            // register handler to force session state initialization
            if (HttpContext.Current.Handler == null)
                HttpContext.Current.Handler = new EnableSessionStateHandler();

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
            // unmap our temporary state handler.
            if (HttpContext.Current.Handler is EnableSessionStateHandler)
                HttpContext.Current.Handler = null;

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
            try
            {
                // last ditch effort to clean up proxy
                if (HttpContext.Current is HttpContext c)
                {
                    if (c.Items.Contains(ContextProxyItemKey))
                    {
                        // attempt to dispose of instance
                        var proxy = c.Items[ContextProxyItemKey] as IDisposable;
                        if (proxy != null)
                            proxy.Dispose();

                        // remove reference to instance
                        c.Items[ContextProxyItemKey] = null;
                    }
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
