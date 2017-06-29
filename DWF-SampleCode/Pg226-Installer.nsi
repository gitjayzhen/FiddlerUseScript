; Page 226 of Debugging with Fiddler. Sample installation script.
; In a NSIS Script, the semi-colon is a comment operator
Name "MyExtension"

; TODO: Set a specific name for your installer’s executable
OutFile "InstallMyExtension.exe"
; Point to an icon to use for the installer, or omit to use the default
Icon "C:\src\MyExt\MyExt.ico"
XPStyle on                      ; Enable visual-styling for a prettier UI

; Explicitly demand admin permissions because we're going to write to
; Program Files. This prevents the "Program Compatibility Assistant" dialog.
; Note, you can use "user" here if you'd like, but then you must only write
; to HKCU and per-user writable locations on disk.
RequestExecutionLevel "admin"

; Maximize compression
SetCompressor /solid lzma

BrandingText "v1.0.1.0"          ; Text shown at the bottom of the Setup window

;
; TODO: Set the install directory to the proper folder. 
;
; To install to the Extensions folder, use:
InstallDir "$PROGRAMFILES\Fiddler2\Scripts\"
InstallDirRegKey HKLM "SOFTWARE\Microsoft\Fiddler2" "LMScriptPath"

; To install to the Inspectors folder, use:
;InstallDir "$PROGRAMFILES\Fiddler2\Inspectors\"
;InstallDirRegKey HKLM "SOFTWARE\Microsoft\Fiddler2" "PluginPath"
;

Section "Main"
SetOutPath "$INSTDIR"

SetOverwrite on
;
; The next line embeds MyExt.dll from the output folder into the installer.
; When the installer runs, the file will be extracted to $INSTDIR\MyExt.dll
;
File "C:\src\MyExt\bin\release\MyExt.dll"
;
;
; TODO: List any other files your extension depends upon here.
; Be sure to also add those files to the list removed by the
; uninstaller at the bottom of this script
;

;
; Write information about the extension to the Add/Remove Programs dialog
;
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MyExt" "DisplayName" "My Fiddler Extension"
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MyExt" "Comments" "My Extension to Fiddler"
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MyExt" "Publisher" 'MyCo'
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MyExt" "InstallLocation" '$INSTDIR'
WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MyExt" "NoModify" 1
WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MyExt" "NoRepair" 1

;
; TODO: Update this line with the name of your uninstaller, set below
; 
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MyExt" "UninstallString" '"$INSTDIR\UninstallMyExt.exe"'

;
; TODO: Update this line to set a *unique* name for your uninstaller.
; Be sure to update the UninstallString value above to match.
; Also, update “Remove the uninstaller executable” line below to match.
;
WriteUninstaller "UninstallMyExt.exe"

SectionEnd ; end of default section

; ----------------------------------------------
; Perhaps surprisingly, this string cannot appear 
; within the Uninstall section itself
UninstallText "This will uninstall My Fiddler Extension from your system"

Section Uninstall
Delete "$INSTDIR\MyExt.dll"
;
; TODO: Delete the other files you installed here.
;

; Remove the uninstaller regkey
DeleteRegKey HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MyExt"
Delete "$INSTDIR\UninstallMyExt.exe"        ; Remove the uninstaller .exe

SectionEnd             ; eof
