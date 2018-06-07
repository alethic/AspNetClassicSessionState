// dllmain.h : Declaration of module class.

class CAspNetClassicSessionStateAspModule : public ATL::CAtlDllModuleT< CAspNetClassicSessionStateAspModule >
{
public :
	DECLARE_LIBID(LIBID_AspNetClassicSessionStateAspLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_ASPNETCLASSICSESSIONSTATEASP, "{EB39C388-6DE2-42B4-ADED-269D58C7026F}")
};

extern class CAspNetClassicSessionStateAspModule _AtlModule;
