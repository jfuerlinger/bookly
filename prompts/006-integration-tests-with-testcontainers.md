# Claude Master Prompt: Integration Tests mit TestContainers für Bookly.Api

Du bist ein erfahrener .NET-Engineer mit Expertise in Integrationstests und Datenbankverifizierung. Implementiere im bestehenden Projekt `Bookly.Api` ein **Integration-Test-Setup mit TestContainers**, das reale Datenbankoperationen gegen eine echte Datenbankinstanz testet.

## Ziel

Schaffe eine zuverlaessige Integrationstestinfrastruktur, um Szenarien wie "Buch mit ISBN hinzufuegen und in der Datenbank verifizieren" automatisiert zu testen.

## Harte Vorgaben

1. **TestContainers-Paket** ist Pflicht fuer Integrationstests.
   - Nicht fuer Unit-Tests verwenden (InMemory bleibt fuer Unit-Tests).
2. Integrationstests laufen gegen eine **echte Datenbank in einem Container** (z. B. PostgreSQL oder SQL Server, abhaengig von Bookly.Core-Konfiguration).
3. Testdatenbank muss automatisch initialisiert und nach Tests beraeubert werden.
4. Keine hartcodierter Secrets in Tests; nutze sichere lokale Verfahren.
5. Integrationstests sind in einem eigenen Testprojekt oder klar getrennten Ordner zu organisieren (z. B. `tests/Bookly.Api.Integration.Tests/`).
6. Existing Unit-Tests (`Bookly.Api.Tests`) bleiben unberuehrt und laufen mit InMemory weiter.
7. Database-Migrations werden vor Tests automatisch angewendet.

## Konkrete Umsetzung

1. Analysiere zuerst:
   - Welche Datenbank verwendet `Bookly.Core` (pruefen: `DbContext`-Konfiguration, `appsettings.json`, Migrations)?
   - Welche TestContainers-Images sind verfuegbar (z. B. `Testcontainers.PostgreSql`, `Testcontainers.MsSql`)?
2. Fuege NuGet-Pakete hinzu in `Directory.Packages.props`:
   - `Testcontainers` (Core-Paket)
   - Spezifisches Paket fuer die gewaehlte Datenbank (z. B. `Testcontainers.PostgreSql`)
   - `Testcontainers.EntityFrameworkCore` (fuer automatische Migration)
3. Erstelle **neues Testprojekt** `tests/Bookly.Api.Integration.Tests/`:
   - `.csproj`-Datei mit Verweisen auf `Bookly.Api`, `Bookly.Core`, TestContainers-Pakete
   - `xunit` oder bestehendes Test-Framework konsistent halten
4. Implementiere **Testinfrastruktur**:
   - `DatabaseFixture` oder `IntegrationTestBase`: Stellt einen gestarteten Container und initialized DbContext bereit
   - Lifecycle-Management: Container vor Tests starten, nach Tests stoppen
   - Connection-String dynamisch vom running Container holen
5. Schreibe mindestens einen **konkreten Integrationtest**: `AddBookWithIsbnAndVerifyInDatabase`:
   - Szenario: `POST /api/books` mit einer gültigen ISBN aufrufen
   - Verifiziere, dass:
     a) Die HTTP-Response erfolgreich ist (2xx Status)
     b) Das Buch in der Antwort korrekt zurückkommt
     c) Das Buch direkt nach dem Request in der Datenbank abrufbar ist (via DbContext)
     d) ISBN, Titel und weitere Metadaten korrekt gespeichert sind
6. Nutze **HttpClient-Integration** fuer den Test:
   - Starten Sie bei Bedarf einen WebApplicationFactory oder einen Test-Server
   - Alternative: Direkter DbContext-Zugriff nach HTTP-Call fuer einfache Verifizierung
7. Halte Aenderungen klein und fokussiert.
8. Nutze bestehende Logging/Telemetrie aus Bookly.ServiceDefaults, wenn verfügbar.

## Verifikation (verpflichtend)

Nach der Implementierung:

1. **Build**:
   - `dotnet build`
   - Keine Compiler-Fehler/-Warnungen im neuen Projekt
2. **Test-Ausfuehrung**:
   - `dotnet test tests/Bookly.Api.Integration.Tests/` muss erfolgreich durchlaufen
   - Der neue `AddBookWithIsbnAndVerifyInDatabase`-Test muss **gruen** werden
   - Alle bestehenden Unit-Tests (`Bookly.Api.Tests`, `Bookly.Core.Tests`) muessen still **gruen** sein
3. **Container-Verifikation**:
   - TestContainers-Container werden korrekt gestartet und gestoppt (via Logs pruefen)
   - Keine orphaned Container bleiben zurueck
4. **Datenbank-Konsistenz**:
   - Test liest Buch direkt aus Datenbank und bestaetigt Praesenzen
   - ISBN und andere Felder sind exakt wie gesendet

## Akzeptanzkriterien

1. Neues Integrationstestprojekt existiert: `tests/Bookly.Api.Integration.Tests/`
2. TestContainers-Infrastruktur ist eingerichtet und funktioniert.
3. Mindestens ein Integrationtest `AddBookWithIsbnAndVerifyInDatabase` existiert:
   - Testet das HTTP POST auf `/api/books` (oder relevanter Endpoint)
   - Verifiziert Response-Status und Payload
   - Query die Datenbank direkt und bestaetigt Buchpraesenzen mit ISBN, Titel, Metadaten
4. Build ist erfolgreich.
5. Alle Tests (Unit + Integration) laufen erfolgreich durch.
6. Bestehende Unit-Tests weiterhin unberuehrt und gruen.
7. Keine Secrets in Code/Logs/Tests.
8. Migrations laufen vor Tests automatisch.
9. TestContainers-Container werden sauber aufgeraeumt.

## Erwartete Abschlussausgabe

Liefere am Ende:

1. Kurze Zusammenfassung der Aenderungen.
   - Welche Datenbank wird fuer Tests verwendet?
   - Welche TestContainers-Pakete hinzugefuegt?
2. Liste aller geaenderten/neuen Dateien.
3. Liste neu installierter/aktualisierter NuGet-Pakete (mit Versionen).
4. Exakte Testlauf-Befehle:
   - Befehl zum Ausfuehren aller Integrationstests
   - Befehl zum Ausfuehren nur des neuen `AddBookWithIsbnAndVerifyInDatabase`-Tests
5. Test-Ausfuehrungsergebnis:
   - Testname(n), Status (bestanden/fehlgeschlagen)
   - Kurze Erlaeuterung der Verifizierungsschritte
6. Kurzer Security- und Performance-Impact-Hinweis:
   - TestContainers-Overhead
   - Keine Produktionsdaten im Test
7. Hinweis auf DbContext-Konfiguration und verwendetes Datenbanksystem.

## Arbeitsmodus

Arbeite selbststaendig und ende erst, wenn alle Akzeptanzkriterien erfuellt sind oder ein echter Blocker mit konkreter Ursache vorliegt.