using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.SessionState;

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
                context.AddOnPostAcquireRequestStateAsync(OnBeginPostAcquireRequestStateAsync, OnEndPostAcquireRequestStateAsync);
                context.AddOnPostRequestHandlerExecuteAsync(OnBeginPostRequestHandlerExecuteAsync, OnEndPostRequestHandlerExecuteAsync);
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
                // session state is always required
                context.SetSessionStateBehavior(SessionStateBehavior.Required);

                // store reference to proxy in context for keep alive
                var proxy = new AspNetStateProxy();
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
        /// Invoked after the session state is acquired.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="cb"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        IAsyncResult OnBeginPostAcquireRequestStateAsync(object sender, EventArgs args, AsyncCallback cb, object extraData)
        {
            // ignore spurious calls
            var context = HttpContext.Current;
            if (context == null)
                return new CompletedAsyncResult(true);

            // only generate for classic ASP requests
            if (context.Request.CurrentExecutionFilePathExtension == ".asp")
            {
                // copy session state into proxy
                var proxy = (AspNetStateProxy)context.Items[ContextProxyPtrKey];
                proxy.State = SaveForAsp(context.Session);
            }

            return new CompletedAsyncResult(true);
        }

        /// <summary>
        /// Returns an enumeration of key value pair as the state should be communicated to ASP.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        byte[] SaveForAsp(HttpSessionState state)
        {
            // transform to ASP format
            var s = state.Cast<string>().Where(i => i.StartsWith("ASP::")).ToDictionary(i => i.Substring("ASP::".Length), i => state[i]);

            // serialize to binary stream
            var m = new MemoryStream();
            var f = new BinaryFormatter();
            f.Serialize(m, s);

            return m.ToArray();
        }

        /// <summary>
        /// Invoked after the session state is acquired.
        /// </summary>
        /// <param name="ar"></param>
        void OnEndPostAcquireRequestStateAsync(IAsyncResult ar)
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
        IAsyncResult OnBeginPostRequestHandlerExecuteAsync(object sender, EventArgs args, AsyncCallback cb, object extraData)
        {
            // ignore spurious calls
            var context = HttpContext.Current;
            if (context == null)
                return new CompletedAsyncResult(true);

            // only generate for classic ASP requests
            if (context.Request.CurrentExecutionFilePathExtension == ".asp")
            {
                // copy session state from proxy
                var proxy = (AspNetStateProxy)context.Items[ContextProxyPtrKey];
                var state = LoadFromAsp(proxy.State);

                // apply new values
                foreach (var kvp in state)
                    context.Session[kvp.Key] = kvp.Value;

                // remove missing values
                foreach (var key in context.Session.Keys.Cast<string>().Where(i => i.StartsWith("ASP::")).ToList())
                    if (state.ContainsKey(key.Substring("ASP::".Length)) == false)
                        context.Session.Remove(key);
            }

            return new CompletedAsyncResult(true);
        }

        /// <summary>
        /// Returns an enumeration of key value pair as the state should be communicated to ASP.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        Dictionary<string, object> LoadFromAsp(byte[] buffer)
        {
            // deserialize incoming data
            var f = new BinaryFormatter();
            var s = (Dictionary<string, object>)f.Deserialize(new MemoryStream(buffer));

            // transform from ASP format to ASP.Net format
            var d = s.ToDictionary(i => "ASP::" + i.Key, i => i.Value);

            return d;
        }

        /// <summary>
        /// Invoked after the request.
        /// </summary>
        /// <param name="ar"></param>
        void OnEndPostRequestHandlerExecuteAsync(IAsyncResult ar)
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

            // only generate for classic ASP requests
            if (context.Request.CurrentExecutionFilePathExtension == ".asp")
            {
                // release proxy
                var proxy = (AspNetStateProxy)context.Items[ContextProxyPtrKey];
                var intPtr = Marshal.GetIUnknownForObject(proxy);
                Marshal.Release(intPtr);
                Marshal.Release(intPtr);
                context.Items[ContextProxyPtrKey] = null;
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
