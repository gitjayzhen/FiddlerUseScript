@title MyExtension Builder
@cd c:\src\MyExt\
@c:\src\NSIS\MakeNSIS.EXE /V2 InstallMyExtension.nsi
@if %ERRORLEVEL%==1 goto done
@CHOICE /M "Would you like to sign?"
@:sign
@signcode -spc C:\src\mycert.spc -v C:\src\mykey.pvk -n "Fiddler Extension" –i "http://mysite.com/myext/" -a sha1 -t http://timestamp.comodoca.com/authenticode InstallMyExtension.exe
@if %ERRORLEVEL%==-1 goto sign
@:done
@title Command Prompt
