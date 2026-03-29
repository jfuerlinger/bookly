# Claude Master Prompt: Initiale Bookly Aspire Projektstruktur

Du bist ein erfahrener .NET- und Aspire-Engineer. Erstelle in diesem Workspace eine vollständige, lauffähige initiale Lösung namens `Bookly`, inklusive Tests und agentischer Verifikation.

## Zielbild

Baue eine .NET Aspire-basierte Multi-Projekt-Lösung mit diesen Projekten:

1. `Bookly.AppHost` (Aspire AppHost, startet alle Services)
2. `Bookly.Api` (ASP.NET Core Web API)
3. `Bookly.Ui` (Blazor Web App)
4. `Bookly.Cli` (Console/CLI-App)
5. `Bookly.Core` (Domäne + Datenschicht + EF Core)
6. `Bookly.Core.Tests` (Unit Tests für Core)
7. `Bookly.Ui.E2E` (Playwright End-to-End Tests für Blazor UI)

## Kontext7 Pflicht

Nutze Context7 als primäre Dokuquelle für Aspire, .NET, Blazor und Entity Framework.

1. Löse zu Beginn die Library-IDs über Context7 auf (falls nicht bekannt).
2. Hinterlege die final verwendeten Referenzen in einer Datei `docs/context7-references.md`.
3. Diese Datei muss mindestens enthalten:
	- Thema (`Aspire`, `.NET`, `Blazor`, `Entity Framework Core`)
	- Context7 Library-ID
	- Kurzbegründung, wofür die Referenz genutzt wurde

## Architektur- und Implementierungsanforderungen

1. Erzeuge eine saubere Solution-Struktur mit sinnvollen Projekt-Referenzen.
2. In `Bookly.Core`:
	- EF Core mit PostgreSQL konfigurieren.
	- Erstelle einen DbContext (z. B. `BooklyDbContext`).
	- Die Datenbank muss `bookly` heißen.
	- Lege mindestens eine einfache Entity an (z. B. `Book`).
3. Erstelle in `Bookly.Core` die Klasse `MathUtils` mit:
	- `public static int Add(int a, int b)`
4. In `Bookly.Api`:
	- Exponiere mindestens einen Health-Endpunkt.
	- Exponiere einen Endpunkt für Addition, der intern `MathUtils.Add` nutzt.
5. In `Bookly.Ui`:
	- Erstelle eine eigene Seite (z. B. `/add`), auf der zwei Zahlen eingegeben werden können.
	- Die Seite soll die Additionsfunktion über die API nutzen und das Ergebnis anzeigen.
6. In `Bookly.Cli`:
	- Implementiere einen simplen Kommandoaufruf (z. B. `add <a> <b>`), der `MathUtils.Add` demonstriert.

## Aspire AppHost Anforderungen

1. `Bookly.AppHost` muss alle relevanten Services starten:
	- PostgreSQL
	- `Bookly.Api`
	- `Bookly.Ui`
	- optional `Bookly.Cli` (falls als Service sinnvoll, sonst dokumentieren)
2. Konfiguriere Service Discovery/Verbindungen so, dass UI und API korrekt laufen.
3. Stelle sicher, dass `Bookly.Core`-Datenzugriff von der API korrekt verdrahtet ist.

## Testanforderungen

1. Unit Test Projekt `Bookly.Core.Tests`:
	- Verifiziere `MathUtils.Add` mit mehreren Testfällen.
2. Blazor Add-Page Test:
	- Erstelle in `Bookly.Ui.E2E` einen dedizierten Playwright-Test nur für die Add-Seite.
	- Teste Eingabe, Ausführung und erwartetes Ergebnis.
3. API-Testbarkeit:
	- Stelle sicher, dass die API-Endpunkte während E2E erreichbar sind.

## Agentic Loop (verpflichtend)

Arbeite in einer Schleife nach folgendem Muster, bis alle Akzeptanzkriterien erfüllt sind:

1. Build und Tests ausführen.
2. Fehler analysieren.
3. Automatisch fixen.
4. Erneut ausführen.

Beende erst, wenn Playwright CLI Tests und die Unit Tests erfolgreich sind und API + Blazor als funktional verifiziert gelten.

## Ausführungsvorgaben

1. Nutze .NET-Version 10 im Workspace, ansonsten dokumentiere Annahme.
2. Nutze übliche .NET CLI Kommandos (`dotnet new`, `dotnet sln`, `dotnet add`, `dotnet test`, etc.).
3. Nutze migrationsbasierten EF-Core-Setup (erste Initial-Migration erstellen).
4. Halte Code minimal, klar und initial-produktionsnah.

## Abnahmekriterien (Definition of Done)

1. Solution mit allen geforderten Projekten existiert.
2. AppHost startet alle benötigten Dienste inkl. PostgreSQL.
3. DB-Name ist `bookly`.
4. `MathUtils.Add` existiert in Core.
5. Unit Tests für `MathUtils.Add` sind grün.
6. Blazor Add-Seite existiert und nutzt API.
7. Dedizierter Playwright E2E Test für Add-Seite ist grün.
8. API + UI wurden via Playwright CLI erfolgreich getestet.
9. `docs/context7-references.md` enthält die hinterlegten Context7-Referenzen.

## Erwartete Ausgabe von dir (Claude)

Liefere am Ende:

1. Eine kurze Änderungszusammenfassung.
2. Die finale Projektstruktur als Baum.
3. Die wichtigsten ausgeführten Kommandos.
4. Den finalen Teststatus (Unit + Playwright).
5. Offene Punkte/Annahmen (falls vorhanden).

## Arbeitsmodus

Starte sofort mit der Umsetzung. Stelle nur dann Rückfragen, wenn eine Blockade besteht, die nicht durch sinnvolle Annahmen auflösbar ist.
