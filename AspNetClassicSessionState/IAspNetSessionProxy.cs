using System.Runtime.InteropServices;

namespace AspNetClassicSessionState
{

    [ComVisible(true)]
    [ComImport]
    [Guid("5E9064BB-A864-45BA-BC67-857946635585")]
    public interface IAspNetSessionProxy
    {

        object this[string key] { get; set; }

        string SessionID { get; }

        int Timeout { get; set; }

        /// <summary>
        /// Gets a value indicating whether the session was created with the current request.
        /// </summary>
        bool IsNewSession { get; }

        void Abandon();

    }

}
