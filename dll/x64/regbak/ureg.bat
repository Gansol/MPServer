gacutil /u MicePowCOMProtocol
regasm MicePowCOMProtocol.dll /unregister

gacutil /u MicePowMemberIO
regsvcs /u MicePowMemberIO.dll

gacutil /u MicePowMemberLogic
regsvcs /u MicePowMemberLogic.dll

gacutil /u MicePowMemberUI
regsvcs /u MicePowMemberUI.dll

gacutil /u Gansol.dll

pause