regasm MPCOM_Data.dll /tlb MPCOM_Data.dll 
gacutil /i MPCOM_Data.dll 

regsvcs MPCOM_IO.dll
gacutil /i MPCOM_IO.dll

regsvcs MPCOM_Logic.dll
gacutil /i MPCOM_Logic.dll

regsvcs  MPCOM_UI.dll
gacutil /i MPCOM_UI.dll

pause
