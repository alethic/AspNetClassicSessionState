// AspNetState.cpp : Implementation of CAspNetState

#include "stdafx.h"
#include "AspNetState.h"

using namespace System;

STDMETHODIMP CAspNetState::OnStartPage(IUnknown* pUnk)
{
    AspNetClassicSessionState::Managed::AspNetState^ state;

    if (!pUnk)
        return E_POINTER;

    state = gcnew AspNetClassicSessionState::Managed::AspNetState();
    state->Load();
    delete state;

    return S_OK;
}

STDMETHODIMP CAspNetState::OnEndPage()
{
    AspNetClassicSessionState::Managed::AspNetState^ state;

    state = gcnew AspNetClassicSessionState::Managed::AspNetState();
    state->Save();
    delete state;

    return S_OK;
}
