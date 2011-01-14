Set shell = CreateObject("WScript.Shell")

' get the user's desktop
desktopdir = shell.RegRead("HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders\Desktop")

' get the user's xaml documents folder
xamldocsdir = shell.RegRead("HKLM\Software\Kaxaml\XamlDocumentsFolder")

' get the kaxaml installation path (to the .exe)
kaxamlpath = shell.RegRead("HKLM\Software\Kaxaml\InstallPath")

' create a shortcut file
shortcutfile = desktopdir & "\Kaxaml.lnk"
set shortcut = shell.CreateShortcut(shortcutfile)

' set parameters
shortcut.TargetPath = kaxamlpath
shortcut.WorkingDirectory = xamldocsdir

' save the shortcut
shortcut.Save

' run Kaxaml for the first time with readme.xaml
shell.run("""" & kaxamlpath & """ """ & xamldocsdir & "\readme.xaml""")

