// AspNetState.cpp : Implementation of CAspNetState

#include "stdafx.h"
#include "AspNetState.h"


// CAspNetState


STDMETHODIMP CAspNetState::OnStartPage(IUnknown* pUnk)
{
    if (!pUnk)
        return E_POINTER;

    CComPtr<IScriptingContext> spContext;
    HRESULT hr;

    // Get the IScriptingContext Interface
    hr = pUnk->QueryInterface(__uuidof(IScriptingContext), (void **)&spContext);
    if (FAILED(hr))
        return hr;

    // Get Request Object Pointer
    hr = spContext->get_Request(&m_piRequest);
    if (FAILED(hr))
    {
        return hr;
    }

    // Get Session Object Pointer
    hr = spContext->get_Session(&m_piSession);
    if (FAILED(hr))
    {
        m_piRequest.Release();
        return hr;
    }

    m_bOnStartPageCalled = TRUE;

    m_state = gcnew AspNetClassicSessionState::Managed::AspNetState();
    m_state->Load();

    return S_OK;
}

STDMETHODIMP CAspNetState::OnEndPage()
{
    m_bOnStartPageCalled = FALSE;

    m_state->Save();
    delete m_state;

    // Release all interfaces
    m_piRequest.Release();
    m_piSession.Release();
    return S_OK;
}
