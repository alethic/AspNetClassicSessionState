// AspNetState.h : Declaration of the CAspNetState

#pragma once

#include "resource.h"       // main symbols
#include <asptlb.h>         // Active Server Pages Definitions
#include <vcclr.h>
#include <msclr\auto_gcroot.h>

#include "AspNetClassicSessionStateAsp_i.h"

using namespace ATL;



// CAspNetState

class ATL_NO_VTABLE CAspNetState :
    public CComObjectRootEx<CComSingleThreadModel>,
    public CComCoClass<CAspNetState, &CLSID_AspNetState>,
    public IDispatchImpl<IAspNetState, &IID_IAspNetState, &LIBID_AspNetClassicSessionStateAspLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
    CAspNetState()
    {

    }

    DECLARE_REGISTRY_RESOURCEID(IDR_ASPNETSTATE)


    BEGIN_COM_MAP(CAspNetState)
        COM_INTERFACE_ENTRY(IAspNetState)
        COM_INTERFACE_ENTRY(IDispatch)
    END_COM_MAP()


    DECLARE_PROTECT_FINAL_CONSTRUCT()

    HRESULT FinalConstruct()
    {
        return S_OK;
    }

    void FinalRelease()
    {

    }

    // IAspNetState
public:
    STDMETHOD(OnStartPage)(IUnknown* IUnk);
    STDMETHOD(OnEndPage)();
private:
};

OBJECT_ENTRY_AUTO(__uuidof(AspNetState), CAspNetState)
