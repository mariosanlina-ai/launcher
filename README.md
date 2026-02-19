# Fiesta Online .EXE Launcher

Dieser Launcher ist jetzt eine **echte Windows Desktop-App (.exe)** auf Basis von **C# WinForms**.

## Funktionen

- `.exe`-Launcher (kein Python-Script)
- Pfad-Auswahl für `Fiesta.exe`
- Speichern von Startargumenten
- Felder für **Server-IP** und **Port**
- Dunkles, moderneres UI-Design
- Speichern der Konfiguration unter `%AppData%/FiestaLauncher/config.json`

## Build (Windows)

```powershell
dotnet build -c Release
```

Ergebnis:

- `bin/Release/net8.0-windows/FiestaLauncher.exe`

## Veröffentlichung als einzelne EXE (optional)

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

Beispielpfad:

- `bin/Release/net8.0-windows/win-x64/publish/FiestaLauncher.exe`
