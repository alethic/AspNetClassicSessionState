using System;
using System.Runtime.InteropServices;

namespace NSession
{

    [ComVisible(true)]
    [Guid("A232ABCA-A611-4789-B338-3D43ABDD43BF")]
    public interface ISessionStateClient
    {

        void GetItem();
        void GetItemExclusive();
        void SetAndReleaseItemExclusive();

    }

}