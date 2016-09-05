gacutil /u MPCOM_Data.dll
regasm MPCOM_Data.dll /unregister

gacutil /u MPCOM_IO.dll
regasm MPCOM_IO.dll /unregister

gacutil /u MPCOM_Logic.dll
regasm MPCOM_Logic.dll /unregister

gacutil /u  MPCOM_UI.dll
regasm  MPCOM_UI.dll /unregister

pause