﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

using ASPTypeLibrary;

namespace AspNetClassicSessionState.Managed
{

    /// <summary>
    /// Managed entry point for COM object.
    /// </summary>
    [Guid("FAF84558-586E-4628-BD2D-456AACE30B46")]
    public class AspNetState : IDisposable
    {

        readonly IRequest request;
        readonly ISessionObject session;
        readonly AspStateProxyWrapper proxy;

        /// <summary>
        /// Initializes the state object.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="session"></param>
        public AspNetState()
        {
            this.request = (IRequest)System.EnterpriseServices.ContextUtil.GetNamedProperty("Request");
            this.session= (ISessionObject)System.EnterpriseServices.ContextUtil.GetNamedProperty("Session");
            this.proxy = CreateProxyWrapper();
        }

        /// <summary>
        /// Loads ASP session from ASP.Net.
        /// </summary>
        public void Load()
        {
            // clear existing ASP session
            session.Contents.RemoveAll();

            // copy session items from ASP.NET to ASP
            var d = (IDictionary<string, object>)proxy.Pull();
            foreach (var kvp in d)
                session[kvp.Key] = kvp.Value;
        }

        /// <summary>
        /// Saves ASP session to ASP.Net.
        /// </summary>
        public void Save()
        {
            // copy session items from ASP to ASP.NET
            var d = new Dictionary<string, object>();
            foreach (string key in session.Contents)
                d[key] = session[key];
            proxy.Push(d);
        }

        /// <summary>
        /// Creates a proxy into the ASP.Net AppDomain.
        /// </summary>
        /// <returns></returns>
        dynamic CreateProxyWrapper()
        {
            // unique ID for the request
            var objRefEnc = (string)((IStringList)request.ServerVariables["HTTP_ASPNETSTATEPROXYREF"])[1];
            if (objRefEnc == null)
                throw new InvalidOperationException("Unable to discover ASP.Net Remote State Proxy. Ensure registration completed.");

            // deserialize proxy reference and connect
            var srs = new BinaryFormatter();
            var aspNetProxy = (IStrongBox)srs.Deserialize(new MemoryStream(Convert.FromBase64String(objRefEnc)));

            return new AspStateProxyWrapper(aspNetProxy);
        }

        /// <summary>
        /// Simple wrapper around remote proxy.
        /// </summary>
        class AspStateProxyWrapper
        {

            readonly IStrongBox proxy;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="proxy"></param>
            public AspStateProxyWrapper(IStrongBox proxy)
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
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

    }

}