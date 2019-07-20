using System;
using System.Runtime.InteropServices;
using System.Web;

namespace AspNetClassicSessionState
{

    /// <summary>
    /// Provides access to get or set the ASP.Net session state items.
    /// </summary>
    [ComVisible(true)]
    [Guid("E3A70CB0-FA23-4123-8BE3-01F85343441F")]
    public class AspNetSessionProxy : IAspNetSessionProxy
    {

        readonly HttpContext context;
        readonly AspNetClassicSessionConfigurationSection config;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="config"></param>
        public AspNetSessionProxy(HttpContext context, AspNetClassicSessionConfigurationSection config)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Gets or sets an object in state.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get => context.Session[config.Prefix + key];
            set => context.Session[config.Prefix + key] = value;
        }

        /// <summary>
        /// Gets the unique session ID.
        /// </summary>
        public string SessionID
        {
            get => context.Session.SessionID;
        }

        /// <summary>
        /// Gets or sets the session timeout.
        /// </summary>
        public int Timeout
        {
            get => context.Session.Timeout;
            set => context.Session.Timeout = value;
        }

        /// <summary>
        /// Gets a value indicating whether the session was created with the current request.
        /// </summary>
        public bool IsNewSession
        {
            get => context.Session.IsNewSession;
        }

        /// <summary>
        /// Abandons the current session.
        /// </summary>
        public void Abandon()
        {
            context.Session.Abandon();
        }

    }

}
