// AspNetState.h : Declaration of the CAspNetState

#pragma once

#include "resource.h"       // main symbols
#include <asptlb.h>         // Active Server Pages Definitions
#include <vcclr.h>

#include "AspNetClassicSessionStateAsp_i.h"

using namespace ATL;



// CAspNetState

class ATL_NO_VTABLE CAspNetState :
    public CComObjectRootEx<CComMultiThreadModel>,
    public CComCoClass<CAspNetState, &CLSID_AspNetState>,
    public IDispatchImpl<IAspNetState, &IID_IAspNetState, &LIBID_AspNetClassicSessionStateAspLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
    CAspNetState()
    {
        m_pUnkMarshaler = nullptr;
    }

    DECLARE_REGISTRY_RESOURCEID(IDR_ASPNETSTATE)


    BEGIN_COM_MAP(CAspNetState)
        COM_INTERFACE_ENTRY(IAspNetState)
        COM_INTERFACE_ENTRY(IDispatch)
        COM_INTERFACE_ENTRY_AGGREGATE(IID_IMarshal, m_pUnkMarshaler.p)
    END_COM_MAP()


    DECLARE_PROTECT_FINAL_CONSTRUCT()
    DECLARE_GET_CONTROLLING_UNKNOWN()

    HRESULT FinalConstruct()
    {
        return CoCreateFreeThreadedMarshaler(GetControllingUnknown(), &m_pUnkMarshaler.p);
    }

    void FinalRelease()
    {
        m_pUnkMarshaler.Release();
    }

    CComPtr<IUnknown> m_pUnkMarshaler;

    // IAspNetState
public:
    STDMETHOD(OnStartPage)(IUnknown* IUnk);
    STDMETHOD(OnEndPage)();
private:
};

OBJECT_ENTRY_AUTO(__uuidof(AspNetState), CAspNetState)
