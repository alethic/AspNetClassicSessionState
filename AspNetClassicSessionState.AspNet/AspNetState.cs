﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
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

            // deserialize from transfer format
            var state = (Dictionary<string, object>)new BinaryFormatter().Deserialize(new MemoryStream(proxy.State));

            // copy session items from ASP.NET to ASP
            foreach (var kvp in state)
                session[kvp.Key] = kvp.Value;

            // remove items no longer present
            foreach (var key in session.Contents.OfType<string>().ToList())
                if (state.ContainsKey(key) == false)
                    session.Contents.Remove(key);
        }

        /// <summary>
        /// Saves ASP session to ASP.Net.
        /// </summary>
        public void OnEndPage()
        {
            // copy compatible items to new dictionary
            var d = new Dictionary<string, object>();
            foreach (var key in session.Contents.OfType<string>())
                d[key] = session[key];

            // serialize to transfer format
            var m = new MemoryStream();
            var f = new BinaryFormatter();
            f.Serialize(m, d);

            // transfer to ASP.Net
            proxy.State = m.ToArray();

            // early release of proxy
            Marshal.ReleaseComObject(proxy);
            proxy = null;
        }

        /// <summary>
        /// Creates a proxy into the ASP.Net AppDomain.
        /// </summary>
        /// <returns></returns>
        dynamic GetProxy()
        {
            // unique ID for the request
            var variables = (IStringList)request.ServerVariables[$"HTTP_{AspNetStateModule.HeadersProxyPtrKey}"];
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
