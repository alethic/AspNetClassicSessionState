using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AspNetClassicSessionState.AspNet
{

    /// <summary>
    /// Provides access to get or set the ASP.Net session state items.
    /// </summary>
    [Guid("E3A70CB0-FA23-4123-8BE3-01F85343441F")]
    [ComVisible(true)]
    public class AspNetStateProxy
    {

        /// <summary>
        /// Reference to the saved ASP state.
        /// </summary>
        public Dictionary<string, object> State { get; set; }

    }

}
