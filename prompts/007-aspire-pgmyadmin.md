# Claude Master Prompt: pgAdmin (pgmyadmin) via Aspire laden

Du bist ein erfahrener .NET- und Aspire-Engineer. Erweitere die bestehende Bookly-App so, dass pgAdmin als eigene Resource mit dem Namen pgmyadmin im Aspire AppHost gestartet wird.

Wichtig: Die Umsetzung muss produktionsnah, sicher und lokal reproduzierbar sein. Keine Demo-Abkuerzungen.

## Ziel

Beim Start der AppHost-Anwendung wird neben PostgreSQL, API und UI auch pgAdmin automatisch durch Aspire gestartet und ist lokal erreichbar.

## Harte Vorgaben

1. Nutze bestehende Aspire-Patterns und halte den aktuellen Stil in AppHost bei.
2. Die pgAdmin-Resource muss im AppHost eindeutig als pgmyadmin benannt sein.
3. Keine Secrets im Klartext in Code, Repository-Konfig oder Logs.
4. Zugangsdaten fuer pgAdmin muessen ueber sichere Konfiguration/Secrets laufen.
5. Lokale Container-Workflows erfolgen mit Podman, nicht Docker.
6. Keine ungefragten Refactorings in anderen Projekten.

## Konkrete Umsetzung

1. Analysiere die bestehende Datei src/Bookly.AppHost/AppHost.cs.
2. Ergaenze die bestehende PostgreSQL-Definition um pgAdmin-Unterstuetzung:
   - Fuege eine pgAdmin-Resource hinzu, die mit PostgreSQL verbunden ist.
   - Resource-Name in Aspire: pgmyadmin.
   - Sichere Default-Konfiguration fuer Login ueber Parameter/Secret-Mechanismus.
3. Falls fuer pgAdmin zusaetzliche NuGet-Abhaengigkeiten noetig sind:
   - Trage Paketversionen zentral in Directory.Packages.props ein.
   - Keine Version-Attribute in .csproj.
4. Stelle sicher, dass API/UI-Resource-Verhalten unveraendert bleibt.
5. Dokumentiere kurz, wie pgAdmin lokal erreichbar ist (Host/Port/URL).

## Security-Vorgaben

1. Kein Hardcoding von Passwort oder sensiblen Zugangsdaten.
2. Secret-Einbindung ueber Aspire-Mechanismen (z. B. Parameter/Secret Store).
3. Keine Ausgabe sensibler Werte in Logs.

## Verifikation (Pflicht)

Fuehre folgende Verifikation mit aktueller Aspire CLI aus:

1. aspire --version
2. aspire start
3. aspire ps
4. aspire describe
5. Optional: aspire wait pgmyadmin --timeout 120
6. Logs pruefen mit aspire logs (oder resourcenspezifisch)
7. Mit aspire stop sauber beenden

Wenn lokal Probleme auftreten, zuerst aspire doctor ausfuehren und den Befund dokumentieren.

## Akzeptanzkriterien

1. AppHost startet eine zusaetzliche Resource pgmyadmin.
2. pgAdmin laeuft zusammen mit PostgreSQL, API und UI ueber Aspire.
3. Keine Secrets im Code/Repo.
4. Build ist erfolgreich.
5. Laufzeit-Verifikation ueber Aspire-Befehle ist erfolgreich.
6. Bestehendes Verhalten von API/UI bleibt intakt.

## Erwartete Abschlussausgabe

1. Kurze Zusammenfassung der Aenderungen.
2. Liste der geaenderten Dateien.
3. Relevante Aspire-Kommandos und Ergebnisstatus.
4. URL/Port fuer den Zugriff auf pgAdmin lokal.
5. Kurzer Security-Hinweis zur Secret-Verwaltung.

## Arbeitsmodus

Arbeite in kleinen, nachvollziehbaren Schritten und stoppe erst, wenn alle Akzeptanzkriterien erfuellt sind oder ein echter Blocker mit konkreter Ursache vorliegt.
