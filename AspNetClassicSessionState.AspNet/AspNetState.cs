using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using ASPTypeLibrary;

namespace AspNetClassicSessionState.AspNet
{

    /// <summary>
    /// Managed entry point for COM object.
    /// </summary>
    [ProgId("AspNetClassicSessionState.AspNetState")]
    [Guid("FAF84558-586E-4628-BD2D-456AACE30B46")]
    [ComVisible(true)]
    public class AspNetState
    {

        IRequest request;
        ISessionObject session;
        dynamic proxy;

        /// <summary>
        /// Loads ASP session from ASP.Net.
        /// </summary>
        public void OnStartPage(ScriptingContext sc)
        {
            request = sc.Request;
            session = sc.Session;

            // attempt to load the AppDomain proxy
            proxy = GetProxy();

            var state = (Dictionary<string, object>)proxy.Load();

            // copy session items from ASP.NET to ASP
            foreach (var kvp in state)
                session[kvp.Key] = kvp.Value;
        }

        /// <summary>
        /// Saves ASP session to ASP.Net.
        /// </summary>
        public void OnEndPage()
        {
            var state = new Dictionary<string, object>();
            foreach (string key in session.Contents)
                state[key] = session.Contents[key];

            // copy session items from ASP to ASP.Net
            proxy.Save(state);

            // final release of RCW
            Marshal.FinalReleaseComObject(proxy);
            proxy = null;
        }

        /// <summary>
        /// Creates a proxy into the ASP.Net AppDomain.
        /// </summary>
        /// <returns></returns>
        dynamic GetProxy()
        {
            // unique ID for the request
            var variables = (IStringList)request.ServerVariables["HTTP_ASPNETSTATEPROXYREF"];
            var intPtrStr = variables?.Count >= 1 ? (string)variables[1] : null;
            if (intPtrStr == null)
                throw new InvalidOperationException("Unable to discover ASP.Net Remote State Proxy CCW. Ensure the module is enabled and configuration is complete.");

            // attempt to parse header value as a long
            if (long.TryParse(intPtrStr, out var intPtrL) == false || intPtrL == 0)
                throw new FormatException("Unable to parse CCW IUnknown pointer.");

            // return the dynamic RCW
            return (dynamic)Marshal.GetObjectForIUnknown(new IntPtr(intPtrL));
        }

    }

}
