// AspNetState.cpp : Implementation of CAspNetState

#include "stdafx.h"
#include "AspNetState.h"


// CAspNetState


STDMETHODIMP CAspNetState::OnStartPage(IUnknown* pUnk)
{
    if (!pUnk)
        return E_POINTER;

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

    return S_OK;
}
