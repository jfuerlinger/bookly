# E2E Tests ausfuehren

Diese Anleitung beschreibt, wie die Playwright-E2E-Tests lokal gestartet werden.

## Voraussetzungen

- .NET SDK (passend zur Solution)
- Podman (Docker wird in diesem Repository nicht verwendet)
- Auf macOS: laufende Podman-VM

Beispiel:

```bash
podman machine init   # nur beim ersten Mal
podman machine start
podman info
```

## 1) Abhaengigkeiten bauen

Im Repository-Root:

```bash
dotnet restore
dotnet build
```

## 2) Playwright-Browser installieren

Nach dem Build das Playwright-Installskript ausfuehren:

```bash
bash tests/Bookly.Ui.E2E/bin/Debug/net10.0/playwright.sh install chromium
```

Hinweis: Falls der Pfad abweicht (z. B. anderes Build-Profil), zuerst `dotnet build tests/Bookly.Ui.E2E` ausfuehren und danach den korrekten `playwright.sh`-Pfad verwenden.

## 3) Anwendungs-Stack starten

In einem separaten Terminal den Aspire-Stack starten:

```bash
dotnet run --project src/Bookly.AppHost
```

Damit werden unter anderem API, UI und die benoetigte Postgres-Instanz gestartet.

## 4) E2E-Tests ausfuehren

In einem zweiten Terminal die Ziel-URLs fuer UI und API setzen und dann die Tests starten:

```bash
BOOKLY_UI_URL="http://localhost:5044" \
BOOKLY_API_URL="http://localhost:5199" \
dotnet test tests/Bookly.Ui.E2E --verbosity normal
```

Alternative: Wenn API/UI auf anderen Ports laufen, nur die beiden Umgebungsvariablen entsprechend anpassen.

## 5) Nur einzelne Tests laufen lassen

```bash
dotnet test tests/Bookly.Ui.E2E --filter "FullyQualifiedName~AddPageTests"
dotnet test tests/Bookly.Ui.E2E --filter "FullyQualifiedName~ScalarDocsTests"
```

## Troubleshooting

- Browser fehlt: Playwright-Installskript erneut ausfuehren.
- Verbindungsfehler zu API/UI: pruefen, ob AppHost laeuft und die gesetzten URLs korrekt sind.
- Container-Probleme: `podman machine status` und `podman info` pruefen.
