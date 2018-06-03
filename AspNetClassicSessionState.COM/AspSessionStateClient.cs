using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ASPTypeLibrary;

namespace AspNetClassicSessionState.COM
{

    [ComVisible(true)]
    [Guid("4F132151-EFD8-4B12-BAD2-F86658585F02")]
    [ClassInterface(ClassInterfaceType.None)]
    public partial class AspSessionStateClient :
        IAspSessionStateClient,
        IDisposable
    {

        readonly Lazy<IRequest> request;
        readonly Lazy<ISessionObject> session;
        readonly Lazy<DynamicAspNetStateProxy> proxy;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public AspSessionStateClient()
        {
            this.request = new Lazy<IRequest>(() => (IRequest)ContextUtil.GetNamedProperty("Request"));
            this.session = new Lazy<ISessionObject>(() => (ISessionObject)ContextUtil.GetNamedProperty("Session"));
            this.proxy = new Lazy<DynamicAspNetStateProxy>(() => CreateProxy());
        }

        public void Load()
        {
            // clear existing ASP session
            session.Value.Contents.RemoveAll();

            // copy session items from ASP.NET to ASP
            var d = (IDictionary<string, object>)proxy.Value.Pull();
            foreach (var kvp in d)
                session.Value[kvp.Key] = kvp.Value;
        }

        public void Save()
        {
            // copy session items from ASP to ASP.NET
            var d = new Dictionary<string, object>();
            foreach (string key in session.Value.Contents)
                d[key] = session.Value[key];
            proxy.Value.Push(d);

            // clear existing ASP session
            session.Value.Contents.RemoveAll();
        }

        /// <summary>
        /// Creates a proxy into the ASP.Net AppDomain.
        /// </summary>
        /// <returns></returns>
        dynamic CreateProxy()
        {
            // load previous stored variable containing AppDomain ID
            var appDomainAppId = ((dynamic)request.Value.ServerVariables["APPL_MD_PATH"])[1];
            var appDomainIdKey = $"{appDomainAppId}_APPDOMAINID";
            var appDomainId = Environment.GetEnvironmentVariable(appDomainIdKey, EnvironmentVariableTarget.Process);

            // environment variable contains integer ID of AppDomain
            if (int.TryParse(appDomainId, out var id) == false)
                throw new InvalidOperationException("Unable to discover hosting ASP.Net AppDomain. Ensure registration completed.");

            // scan process for AppDomain
            var appDomain = GetAppDomains().FirstOrDefault(i => i.Id == id);
            if (appDomain == null)
                throw new InvalidOperationException("Unable to discover hosting ASP.Net AppDomain. Ensure registration completed.");

            var a = request.Value.ServerVariables["HTTP_ASPNETSTATEID"];
            var c = a[1];

            var aspNetProxy = (IStrongBox)appDomain.CreateInstanceAndUnwrap(
                "AspNetClassicSessionState.AspNet",
                "AspNetClassicSessionState.AspNet.AspNetStateProxy",
                false,
                BindingFlags.Default,
                null,
                new[] { (string)((IStringList)request.Value.ServerVariables["HTTP_ASPNETSTATEID"])[1] },
                null,
                null);
            if (aspNetProxy == null)
                throw new InvalidOperationException("Unable to generate remote AspNetStateProxy in AppDomain.");

            return new DynamicAspNetStateProxy(aspNetProxy);
        }

        class DynamicAspNetStateProxy
        {

            readonly IStrongBox proxy;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="proxy"></param>
            public DynamicAspNetStateProxy(IStrongBox proxy)
            {
                this.proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
            }

            public void Push(Dictionary<string, object> items)
            {
                proxy.Value = items;
            }

            public Dictionary<string, object> Pull()
            {
                return (Dictionary<string, object>)proxy.Value;
            }

        }

        /// <summary>
        /// Gets a list of AppDomains within the current process.
        /// </summary>
        /// <returns></returns>
        IEnumerable<AppDomain> GetAppDomains()
        {
            var appDomainList = new List<AppDomain>();
            var appCorRuntime = (ICorRuntimeHost)new CorRuntimeHost();

            try
            {
                appCorRuntime.EnumDomains(out var enumeration);

                try
                {
                    object nextDomain = null;
                    appCorRuntime.NextDomain(enumeration, ref nextDomain);

                    while (nextDomain != null)
                    {
                        appDomainList.Add((AppDomain)nextDomain);
                        nextDomain = null;
                        appCorRuntime.NextDomain(enumeration, ref nextDomain);
                    }
                }
                finally
                {
                    appCorRuntime.CloseEnum(enumeration);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(appCorRuntime);
            }

            return appDomainList;
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

    }

}