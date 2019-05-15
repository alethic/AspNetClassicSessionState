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

        void Abandon();

    }

}
