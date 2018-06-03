using System;
using System.Runtime.InteropServices;

namespace AspNetClassicSessionState.COM
{

    [ComVisible(true)]
    [Guid("A232ABCA-A611-4789-B338-3D43ABDD43BF")]
    public interface IAspSessionStateClient
    {

        /// <summary>
        /// Loads ASP Classic Session State from the associated ASP.NET session.
        /// </summary>
        void Load();

        /// <summary>
        /// Saves ASP Classic Session State to the associated ASP.NET session.
        /// </summary>
        void Save();

    }

}