using System;
using System.Configuration;
using System.EnterpriseServices;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;

using ASPTypeLibrary;

namespace NSession
{

    [ComVisible(true)]
    [Guid("B3EB0C5E-5D25-453F-9080-33D3A5CBDB5A")]
    [ClassInterface(ClassInterfaceType.None)]
    public class AspNetSessionStateClient :
        ISessionStateClient,
        IDisposable
    {

        static dynamic store;
        static string cookieName;

        bool exclusive = false;

        IRequest request;
        ISessionObject session;
        HttpContext context;
        string sessionId;

        bool locked;
        TimeSpan lockAge;
        object lockId;
        SessionStateActions actionFlags;
        bool newItem;

        TimeSpan executionTimeout = new TimeSpan(0, 1, 50);
        int sessionTimeout = 20;

        bool disposed;

        public void GetItem()
        {
            GetItemInternal(false);
        }

        public void GetItemExclusive()
        {
            GetItemInternal(true);
        }

        public void SetAndReleaseItemExclusive()
        {
            if (exclusive)
            {
                var sessionItems = new SessionStateItemCollection();
                foreach (string key in session.Contents)
                    sessionItems[key] = session[key];

                var data = new SessionStateStoreData(sessionItems, null, sessionTimeout);
                store.SetAndReleaseItemExclusive(context, sessionId, data, lockId, newItem);
                exclusive = false;
            }
        }

        void GetItemInternal(bool isExclusive)
        {
            Init();

            SessionStateStoreData data;
            if (isExclusive)
            {
                exclusive = isExclusive;
                while (true)
                {
                    data = store.GetItemExclusive(context, sessionId, out locked, out lockAge, out lockId, out actionFlags);
                    if (data == null)
                    {
                        if (locked)
                        {
                            if (lockAge > executionTimeout)
                                store.ReleaseItemExclusive(context, sessionId, lockId);
                            else
                                System.Threading.Thread.Sleep(500);
                        }
                        else
                        {
                            data = store.CreateNewStoreData(context, sessionTimeout);
                            break;
                        }
                    }
                    else
                        break;
                }
            }
            else
                data = store.GetItem(context, sessionId, out locked, out lockAge, out lockId, out actionFlags);

            session.Contents.RemoveAll();

            if (data != null)
            {
                newItem = false;
                var sessionItems = data.Items;

                foreach (string key in sessionItems.Keys)
                    session[key] = sessionItems[key];
            }
            else
                newItem = true;
        }

        /// <summary>
        /// Initializes the request.
        /// </summary>
        void Init()
        {
            if (request == null)
            {
                // classic ASP objects
                request = (IRequest)ContextUtil.GetNamedProperty("Request");
                session = (ISessionObject)ContextUtil.GetNamedProperty("Session");

                // ASP.Net objects
                context = new HttpContext(new AspWorkerRequest(request));

                // store configuration should only happen once
                if (store == null)
                    OneTimeInit();

                // ASP.Net Session Cookie
                var cookie = (IReadCookie)request.Cookies[cookieName];
                sessionId = cookie[Missing.Value];
            }
        }

        /// <summary>
        /// Initializes the AppDomain.
        /// </summary>
        void OneTimeInit()
        {
            var map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = (string)request.ServerVariables["APPL_PHYSICAL_PATH"][1] + "web.config";

            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            var sessionStateSection = (SessionStateSection)config.GetSection("system.web/sessionState");
            if (sessionStateSection != null)
                cookieName = sessionStateSection.CookieName;

            var httpRuntimeSection = config.GetSection("httpRuntime") as HttpRuntimeSection;
            if (httpRuntimeSection != null)
                executionTimeout = httpRuntimeSection.ExecutionTimeout;

            if (sessionStateSection.Mode != SessionStateMode.StateServer)
                throw new InvalidOperationException("AspNetSessionState integration requires OutOfProc session mode.");

            var appDomainAppId = (string)request.ServerVariables["APPL_MD_PATH"][1];
            var uriBaseKey = $"{appDomainAppId}_OUTOFPROCSESSIONSTATESTORE_URIBASE";
            var uriBase = Environment.GetEnvironmentVariable(uriBaseKey, EnvironmentVariableTarget.Process);

            // configure AppDomain store with same uri base as ASP.NET context
            var storeType = typeof(HttpContext).Assembly.GetType("System.Web.SessionState.OutOfProcSessionStateStore");
            SetPrivateStaticField(storeType, "s_uribase", uriBase);

            var connstr = sessionStateSection.StateConnectionString;
            store = Activator.CreateInstance(storeType);

            // force-init session store with configuration
            var createPartionInfo = storeType.GetMethod("CreatePartitionInfo", BindingFlags.Instance | BindingFlags.NonPublic);
            var partitionInfo = (dynamic)createPartionInfo.Invoke(store, new object[] { connstr });
            SetPrivateInstanceField(storeType, "_partitionInfo", store, partitionInfo);

            // configured session timeout
            sessionTimeout = (int)sessionStateSection.Timeout.TotalMinutes;
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    if (exclusive)
                        SetAndReleaseItemExclusive();

                exclusive = false;
                disposed = true;
            }
        }

        static void SetPrivateStaticField(Type type, string member, object value)
        {
            var fi = type.GetField(member, BindingFlags.Static | BindingFlags.NonPublic);
            fi.SetValue(type, value);
        }

        static object GetPrivateStaticField(Type type, string member)
        {
            var fi = type.GetField(member, BindingFlags.Static | BindingFlags.NonPublic);
            return fi.GetValue(type);
        }

        static void SetPrivateInstanceField(Type type, string member, object instance, object value)
        {
            var fi = type.GetField(member, BindingFlags.Instance | BindingFlags.NonPublic);
            fi.SetValue(instance, value);
        }

    }

}