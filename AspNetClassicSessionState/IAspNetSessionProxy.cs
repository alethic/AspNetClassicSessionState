using System.Runtime.InteropServices;

namespace AspNetClassicSessionState
{

    [ComVisible(true)]
    [ComImport]
    [Guid("5E9064BB-A864-45BA-BC67-857946635585")]
    public interface IAspNetSessionProxy
    {

        string SessionID { get; }

        object this[string key] { get; set; }

        void Remove(string key);

        void RemoveAll();

        int Timeout { get; set; }

        /// <summary>
        /// Gets a value indicating whether the session was created with the current request.
        /// </summary>
        bool IsNewSession { get; }

        /// <summary>
        /// Gets a value indicating whether the session is read-only.
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets a value indicating whether access to the collection of session-state values is synchronized (thread safe).
        /// </summary>
        bool IsSynchronized { get; }

        void Abandon();
    }

}
