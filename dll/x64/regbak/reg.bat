regasm MicePowCOMProtocol.dll /tlb MicePowCOMProtocol.tlb
gacutil /i MicePowCOMProtocol.dll

regsvcs MicePowMemberIO.dll
gacutil /i MicePowMemberIO.dll

regsvcs MicePowMemberLogic.dll
gacutil /i MicePowMemberLogic.dll

regsvcs MicePowMemberUI.dll
gacutil /i MicePowMemberUI.dll

gacutil /i Gansol.dll

pause