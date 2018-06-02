using System;
using System.Reflection;
using System.Web;
using System.Web.SessionState;

using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: WebActivatorEx.PreApplicationStartMethod(
    typeof(WebApplication3.App_Start.AspClassicStateModule),
    nameof(WebApplication3.App_Start.AspClassicStateModule.RegisterModule))]

namespace WebApplication3.App_Start
{

    /// <summary>
    /// Initializes the application to share state with ASP Classic Requests through the ASP.Net State Server.
    /// </summary>
    public class AspClassicStateModule :
        IHttpModule
    {

        /// <summary>
        /// Registers the HTTP module.
        /// </summary>
        public static void RegisterModule()
        {
            DynamicModuleUtility.RegisterModule(typeof(AspClassicStateModule));
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
            context.AddOnMapRequestHandlerAsync(OnBeginMapRequestHandlerAsync, OnEndMapRequestHandlerAsync);
            context.AddOnAcquireRequestStateAsync(OnBeginAcquireRequestStateAsync, OnEndAcquireRequestStateAsync);
            context.AddOnPostAcquireRequestStateAsync(OnBeginPostAcquireRequestStateAsync, OnEndPostAcquireRequestStateAsync);
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
            var uriBaseKey = $"{HttpRuntime.AppDomainAppId}_OUTOFPROCSESSIONSTATESTORE_URIBASE";
            var uriBase = Environment.GetEnvironmentVariable(uriBaseKey, EnvironmentVariableTarget.Process);

            // not yet initialized
            if (string.IsNullOrEmpty(uriBase))
            {
                foreach (string moduleName in HttpContext.Current.ApplicationInstance.Modules)
                {
                    var module = HttpContext.Current.ApplicationInstance.Modules[moduleName] as SessionStateModule;
                    if (module != null)
                    {
                        var storeType = typeof(HttpContext).Assembly.GetType("System.Web.SessionState.OutOfProcSessionStateStore");
                        if (storeType == null)
                            throw new InvalidOperationException();

                        // identify current Session State Store type
                        var storeField = typeof(SessionStateModule).GetField("_store", BindingFlags.Instance | BindingFlags.NonPublic);
                        var store = (SessionStateStoreProviderBase)storeField.GetValue(module);

                        // we require the OutOfPrc session state store
                        if (storeType.IsInstanceOfType(store))
                        {
                            // load known uriBase
                            var uriBaseField = storeType.GetField("s_uribase", BindingFlags.Static | BindingFlags.NonPublic);
                            uriBase = (string)uriBaseField.GetValue(null);

                            // set in process
                            Environment.SetEnvironmentVariable(uriBaseKey, uriBase, EnvironmentVariableTarget.Process);
                        }
                    }
                }
            }

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
