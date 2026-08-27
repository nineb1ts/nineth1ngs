# nineth1ngs

## Starten

Voraussetzungen:

- Windows
- .NET 10 SDK

Aus dem Repository-Ordner starten:

```powershell
dotnet run --project .\nineth1ngs.csproj
```

Alternativ die kompilierte Anwendung starten:

```powershell
Start-Process .\bin\Debug\net10.0-windows\nineth1ngs.exe
```

Die lokale SQLite-Datenbank liegt unter:

```text
%LOCALAPPDATA%\nineth1ngs\nineth1ngs.db
```

Beim Start werden fehlende Datenbankordner automatisch erstellt und EF-Core-Migrationen angewendet. Der Fensterzustand liegt getrennt unter `%LOCALAPPDATA%\nineth1ngs\settings.json`.

## Tests

```powershell
dotnet test .\tests\nineth1ngs.Tests\nineth1ngs.Tests.csproj
```
