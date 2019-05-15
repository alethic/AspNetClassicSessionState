using System;
using System.Globalization;
using System.Runtime.InteropServices;

using ASPTypeLibrary;

namespace AspNetClassicSessionState
{

    /// <summary>
    /// Managed entry point for COM object.
    /// </summary>
    [ComVisible(true)]
    [Guid("FAF84558-586E-4628-BD2D-456AACE30B46")]
    [ProgId("AspNetClassicSessionState.AspNetSession")]
    public class AspNetSession
    {

        /// <summary>
        /// Gets or sets an item in session state by the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get => GetProxy()[key];
            set => GetProxy()[key] = value;
        }

        /// <summary>
        /// Gets the unique session ID.
        /// </summary>
        public string SessionID
        {
            get => GetProxy().SessionID;
        }

        /// <summary>
        /// Abandons the session.
        /// </summary>
        public void Abandon()
        {
            GetProxy().Abandon();
        }

        /// <summary>
        /// Discovers the proxy by examining the request context.
        /// </summary>
        /// <returns></returns>
        static IAspNetSessionProxy GetProxy()
        {
            var request = (IRequest)System.EnterpriseServices.ContextUtil.GetNamedProperty("Request");
            if (request == null)
                throw new InvalidOperationException("Unable to locate request context.");

            // unique ID for the request
            var variables = (IStringList)request.ServerVariables["HTTP_" + AspNetClassicSessionModule.HeaderProxyPtrKey];
            var intPtrEnc = variables?.Count >= 1 ? (string)variables[1] : null;
            if (intPtrEnc == null || string.IsNullOrWhiteSpace(intPtrEnc))
                return null;

            // decode pointer to proxy
            var intPtr = new IntPtr(long.Parse(intPtrEnc, NumberStyles.HexNumber));
            if (intPtr == IntPtr.Zero)
                return null;

            // get reference to proxy
            var proxy = (IAspNetSessionProxy)Marshal.GetObjectForIUnknown(intPtr);
            return proxy;
        }

    }

}
