# Dev Tunnel einrichten

Diese Anleitung beschreibt, wie der Microsoft Dev Tunnel konfiguriert wird, um die `bookly-ui` von außen erreichbar zu machen – z. B. für Tests auf einem Mobilgerät oder mit externen Testern.

Der Tunnel wird über den Aspire AppHost verwaltet: Er startet automatisch als Aspire-Ressource, sobald er aktiviert ist, und seine URL ist im **Aspire Dashboard** sichtbar.

---

## Voraussetzungen

### 1. devtunnel CLI installieren

**macOS:**

```bash
brew install devtunnel
```

Alternativ via curl (alle Plattformen):

```bash
curl -sL https://aka.ms/DevTunnelCliInstall | bash
```

**Windows:**

```powershell
winget install Microsoft.devtunnel
```

**Linux:**

```bash
curl -sL https://aka.ms/DevTunnelCliInstall | bash
```

Installation prüfen:

```bash
devtunnel --version
```

### 2. Mit Microsoft-Konto einloggen (einmalig)

```bash
devtunnel login
```

Ein Browser öffnet sich zur Authentifizierung. Nach erfolgreichem Login wird die Session lokal gespeichert – dieser Schritt ist nur einmalig notwendig.

> **Hinweis:** Der Login ist nur für den *Host* (dich) erforderlich. Besucher können dank `--allow-anonymous` ohne Konto auf den Tunnel zugreifen.

---

## Tunnel aktivieren

Der Tunnel ist standardmäßig **deaktiviert** (`DevTunnel:Enabled = false`). Zur Aktivierung stehen zwei Möglichkeiten zur Verfügung:

### Option A: User Secrets (empfohlen)

Im AppHost-Projektverzeichnis:

```bash
cd src/Bookly.AppHost
dotnet user-secrets set "DevTunnel:Enabled" "true"
```

Deaktivierung:

```bash
dotnet user-secrets set "DevTunnel:Enabled" "false"
```

### Option B: appsettings.Development.json (lokaler Override)

In `src/Bookly.AppHost/appsettings.Development.json` den Wert ändern:

```json
"DevTunnel": {
  "Enabled": true
}
```

> **Achtung:** Diese Datei liegt im Repository. Committe `Enabled: true` nicht aus Versehen.

---

## Tunnel verwenden

### Stack starten

```bash
dotnet run --project src/Bookly.AppHost
```

Wenn `DevTunnel:Enabled = true`, erscheint `dev-tunnel` als eigene Ressource im Aspire Dashboard.

### Tunnel-URL ablesen

1. Aspire Dashboard öffnen (URL wird im Terminal beim Start angezeigt)
2. In der Ressourcenliste auf **dev-tunnel** klicken
3. Im Log-Stream erscheint eine Zeile wie:

   ```
   Connect via browser: https://xxxxxxxx-5044.euw.devtunnels.ms
   ```

Diese URL ist extern erreichbar und kann geteilt werden.

---

## Hinweise

### Blazor Interactive Server & WebSockets

`bookly-ui` nutzt Blazor Interactive Server Rendering, das auf WebSocket-Verbindungen angewiesen ist. Der `devtunnel` unterstützt WebSockets nativ – die interaktiven Blazor-Komponenten funktionieren über den Tunnel ohne Einschränkungen.

### Anonymer Zugriff

Der Tunnel wird mit `--allow-anonymous` gestartet. Das bedeutet:
- Besucher benötigen kein Microsoft-Konto
- Der Link kann direkt geteilt werden
- Es gibt keine zusätzliche Authentifizierung vor dem Laden der Seite

### Sicherheitshinweis

- **Kein Produktionseinsatz:** Dev Tunnels sind für lokale Entwicklung und kurzfristige Tests gedacht, nicht als dauerhafter Zugangspunkt.
- **Tunnel beenden:** Sobald der Aspire-Stack gestoppt wird (`Ctrl+C`), wird der Tunnel automatisch beendet.
- **Nicht committen:** `DevTunnel:Enabled = true` nicht in `appsettings.Development.json` einchecken. User Secrets sind die sichere Alternative.

---

## Troubleshooting

### `devtunnel: command not found`

CLI ist nicht in PATH. Installation wiederholen und ggf. Shell neu starten.

### "Login token expired." in den Logs

Das devtunnel-Token ist abgelaufen. Neu einloggen und Aspire neu starten:

```bash
devtunnel login
```

Alternativ mit GitHub-Konto:

```bash
devtunnel login -d github
```

### Tunnel startet nicht / `dev-tunnel` bleibt im Aspire Dashboard in "Starting"

1. Prüfen, ob der Login aktiv ist: `devtunnel list` (zeigt vorhandene Tunnel)
2. Falls abgelaufen: `devtunnel login` erneut ausführen
3. Logs der `dev-tunnel`-Ressource im Aspire Dashboard prüfen

### Seite lädt, aber Blazor reagiert nicht

WebSocket-Verbindung prüfen. Im Browser-DevTools (Network-Tab) nach `_blazor` WebSocket-Verbindungen suchen. Falls blockiert, einen anderen Browser oder das Gerät direkt mit dem Tunnel-Link testen.

### Port-Konflikt (Port 5044 belegt)

```bash
lsof -i :5044   # macOS/Linux
```

Den blockierenden Prozess beenden oder Aspire neu starten.
