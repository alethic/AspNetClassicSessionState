using System;
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

        /// <summary>
        /// Registers the HTTP module.
        /// </summary>
        public static void RegisterModule()
        {
            DynamicModuleUtility.RegisterModule(typeof(AspNetStateModule));
        }

        /// <summary>
        /// Registered as the handler briefly to force initialization of session state.
        /// </summary>
        class EnableSessionStateHandler :
            IHttpHandler,
            IRequiresSessionState
        {

            public bool IsReusable => true;

            public void ProcessRequest(HttpContext context)
            {
                // noop
            }

        }

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        /// <param name="context"></param>
        public void Init(HttpApplication context)
        {
            context.AddOnBeginRequestAsync(BeginOnBeginRequestAsync, EndOnBeginRequestAsync);
            context.AddOnMapRequestHandlerAsync(OnBeginMapRequestHandlerAsync, OnEndMapRequestHandlerAsync);
            context.AddOnAcquireRequestStateAsync(OnBeginAcquireRequestStateAsync, OnEndAcquireRequestStateAsync);
            context.AddOnPostAcquireRequestStateAsync(OnBeginPostAcquireRequestStateAsync, OnEndPostAcquireRequestStateAsync);
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
            var r = Guid.NewGuid().ToString("N");
            HttpContext.Current.Response.Write("");
            HttpContext.Current.Request.Headers.Add("ASPNETSTATEID", r);
            HttpContext.Current.Response.Flush();
            AspNetStateProxy.SetContext(r, HttpContext.Current);

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
        /// <param name="e"></param>
        /// <param name="cb"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        IAsyncResult OnBeginAcquireRequestStateAsync(object sender, EventArgs e, AsyncCallback cb, object extraData)
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
        /// Invoked after the session state is acquired.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="cb"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        IAsyncResult OnBeginPostAcquireRequestStateAsync(object sender, EventArgs args, AsyncCallback cb, object extraData)
        {
            // use an environment variable to communicate Session State Module URI base between ASP.NET and ASP Classic
            var appDomainIdKey = $"{HttpRuntime.AppDomainAppId}_APPDOMAINID";
            var appDomainId = Environment.GetEnvironmentVariable(appDomainIdKey, EnvironmentVariableTarget.Process);

            // register environment variable used to discover AppDomain
            if (string.IsNullOrEmpty(appDomainId))
                Environment.SetEnvironmentVariable(appDomainIdKey, AppDomain.CurrentDomain.Id.ToString(), EnvironmentVariableTarget.Process);

            return new CompletedAsyncResult(true);
        }

        /// <summary>
        /// Invoked after the session state is acquired.
        /// </summary>
        /// <param name="ar"></param>
        void OnEndPostAcquireRequestStateAsync(IAsyncResult ar)
        {

        }

        public void Dispose()
        {

        }

    }

}
