gacutil /u MPCOMProtocol
regasm MPCOMProtocol.dll /unregister

gacutil /u COMMemberIO.dll
regasm COMMemberIO.dll /unregister

gacutil /u COMMemberLogic.dll
regasm COMMemberLogic.dll /unregister

gacutil /u COMMemberUI.dll
regasm COMMemberUI.dll /unregister

pause