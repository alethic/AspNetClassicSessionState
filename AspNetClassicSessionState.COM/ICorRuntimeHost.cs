using System;
using System.Runtime.InteropServices;

namespace AspNetClassicSessionState.COM
{

    [Guid("CB2F6722-AB3A-11D2-9C40-00C04FA30A3E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface ICorRuntimeHost
    {

        void CreateLogicalThreadState();

        void DeleteLogicalThreadState();

        void SwitchInLogicalThreadState();

        void SwitchOutLogicalThreadState();

        void LocksHeldByLogicalThread();

        void MapFile();

        void GetConfiguration();

        void Start();

        void Stop();

        void CreateDomain();

        void GetDefaultDomain();

        void EnumDomains(out IntPtr enumHandle);

        void NextDomain(IntPtr enumHandle, [MarshalAs(UnmanagedType.IUnknown)]ref object appDomain);

        void CloseEnum(IntPtr enumHandle);

        void CreateDomainEx();

        void CreateDomainSetup();

        void CreateEvidence();

        void UnloadDomain();

        void CurrentDomain();

    }

}