# Claude Master Prompt: ISBN-Scan mit Metadaten-Fallbacks und EF-Core-Migrationen

Du bist ein erfahrener .NET-, Aspire-, API- und Blazor-Engineer. Implementiere im bestehenden Projekt die Funktion, Buecher per Kamera-Scan einer ISBN zu erfassen, Metadaten ueber frei verfuegbare APIs nachzuladen und das Buch in der persoenlichen Bibliothek zu speichern.

Wichtig: Die Umsetzung muss produktionsreif, robust, testbar und observability-faehig sein. Keine Demo-Loesung.

## Ziel

Ein User kann in der UI eine ISBN per Kamera scannen. Das System recherchiert Buch-Metadaten ueber mindestens zwei kostenfreie, oeffentlich erreichbare APIs mit Fallback-Strategie. Das Ergebnis wird als Buch in der persoenlichen Bibliothek persistiert.

## Harte Vorgaben

1. Verwende mindestens zwei verschiedene kostenfreie, oeffentlich erreichbare APIs als Metadatenquellen.
2. Definiere eine klare Fallback-Reihenfolge mit Timeouts, Retry und sauberer Fehlerbehandlung.
3. Speichere das gescannte Buch inklusive Metadaten in der persoenlichen Bibliothek.
4. Beruecksichtige ein sauberes EF-Core-Datenmodell und erstelle die noetige Migration.
5. Halte Architekturgrenzen ein:
- UI scannt/erfasst ISBN und ruft API auf.
- API orchestriert Lookup/Fallback.
- Core enthaelt Domain, Persistenz und Migrationslogik.
6. Keine Secrets in Code, Konfiguration oder Logs.
7. Nur benoetigte Aenderungen, keine ungefragten Gross-Refactorings.

## Verbindliche API-Quellen (kostenfrei)

Implementiere mindestens diese zwei Quellen:

1. Open Library Books API
- Endpoint-Beispiel: `https://openlibrary.org/api/books?bibkeys=ISBN:{isbn}&format=json&jscmd=data`
- Vorteil: kostenfrei, ohne API-Key nutzbar.

2. Google Books API
- Endpoint-Beispiel: `https://www.googleapis.com/books/v1/volumes?q=isbn:{isbn}`
- Vorteil: kostenfrei nutzbar fuer Basisabfragen (ohne kostenpflichtige Verpflichtung fuer den Scope dieses Features).

Optionaler dritter Fallback (nur wenn sinnvoll): OpenBD.

## Funktionale Anforderungen

1. ISBN-Erfassung
- UI bietet Scan-Flow (Kamera) plus manuellen Fallback-Eingabepfad.
- ISBN-10 und ISBN-13 validieren und normalisieren.
- Ungueltige ISBNs liefern nutzerfreundliche Fehlermeldung nah am Eingabefeld.

2. Lookup-Orchestrierung
- API-Endpunkt nimmt normalisierte ISBN entgegen.
- Primarquelle abfragen, bei Fehler/Timeout/leerem Ergebnis automatisch Fallback-Quelle nutzen.
- Wenn beide Quellen keine Treffer liefern: strukturierte Not-Found-Antwort.

3. Metadaten-Mapping
- Einheitliches internes DTO/Domain-Modell fuer externe Datenquellen.
- Mapping fuer mindestens:
- ISBN-10, ISBN-13
- Titel, Untertitel
- Autoren (1..n)
- Verlag
- Erscheinungsdatum
- Sprache
- Seitenzahl
- Beschreibung/Kurztext
- Cover-URL (small/medium/large, sofern vorhanden)
- Quelle der Metadaten (z. B. `OpenLibrary`, `GoogleBooks`)

4. Persistierung in persoenlicher Bibliothek
- Beim erfolgreichen Lookup wird das Buch in der persoenlichen Bibliothek gespeichert.
- Deduplizierung ueber normalisierte ISBN (mindestens ISBN-13, falls verfuegbar).
- Wenn Buch bereits existiert: kein doppelter Datensatz, stattdessen bestehendes Buch zurueckgeben oder sinnvoll aktualisieren (Regel klar dokumentieren).

## Datenmodell und EF Core (Pflicht)

Erweitere/erstelle ein passendes Datenmodell in `Bookly.Core`.

Mindestens erforderlich:

1. Entity `Book` erweitern (oder neu, falls noch nicht vorhanden) um:
- `Id`
- `Isbn10` (nullable)
- `Isbn13` (nullable)
- `NormalizedIsbn` (required)
- `Title` (required)
- `Subtitle` (nullable)
- `Publisher` (nullable)
- `PublishedOn` (nullable, DateOnly oder DateTime nach bestehender Konvention)
- `Language` (nullable)
- `PageCount` (nullable)
- `Description` (nullable)
- `CoverSmallUrl` (nullable)
- `CoverMediumUrl` (nullable)
- `CoverLargeUrl` (nullable)
- `MetadataSource` (required)
- `CreatedAtUtc` (required)
- `UpdatedAtUtc` (required)

2. Autoren-Beziehung
- Entweder normalisierte Beziehung ueber eigene `Author`-Entity + Join-Table
- oder (wenn im Projektstil passender) eine klar begruendete Alternative.
- Bevorzuge normalisierte Loesung fuer Suchbarkeit und Datenqualitaet.

3. Indizes und Constraints
- Unique Index auf `NormalizedIsbn`.
- Sinnvolle Laengen- und Required-Constraints via Fluent API.
- Optionaler Suchindex auf `Title`.

4. Migrationen
- EF-Core-Migration fuer neue/erweiterte Tabellen erstellen.
- Migration muss deterministisch und rollback-faehig sein.
- Datenbank-Update lokal verifizieren.

## API-Design

1. Neuer Endpoint, z. B. `POST /api/library/isbn-scan`.
2. Request:
- ISBN-String (roh)
3. Response:
- Gespeichertes Buch (stabiler DTO-Contract)
- Bei Fehlern konsistente ProblemDetails (RFC 7807).
4. Validierung:
- Input Validation an der API-Grenze.
- Keine internen Stacktraces in Responses.

## Resilience und Observability

1. Externe API-Calls mit:
- kurzem Timeout
- begrenzten Retries
- optional Circuit Breaker (falls bereits im Projektmuster genutzt)
2. Structured Logging mit Correlation/Trace IDs.
3. Telemetrie fuer:
- Lookup gestartet
- Quelle erfolgreich
- Fallback aktiviert
- Lookup fehlgeschlagen
- Persistierung erfolgreich/fehlgeschlagen

## UI/UX-Anforderungen

1. Klarer Scan-Flow mit Loading-, Error-, Success-States.
2. Manueller ISBN-Eingabefallback immer verfuegbar.
3. Barrierearme Bedienbarkeit:
- Tastaturbedienung
- sichtbarer Focus-State
- valide, hilfreiche Fehlermeldungen
4. Nach Erfolg direkte Rueckmeldung, dass Buch in Bibliothek gespeichert wurde.

## Testanforderungen

1. Unit Tests
- ISBN-Validierung/Normalisierung (inkl. Edge Cases)
- Mapping der externen API-Antworten
- Fallback-Orchestrierung (Primary fail -> Secondary success)
- Deduplizierungslogik

2. Integrations-Tests (API + Persistenz)
- erfolgreicher Scan speichert Buch
- bereits vorhandene ISBN erzeugt kein Duplikat
- beide Quellen ohne Treffer liefern konsistentes NotFound

3. Hinweis
- E2E-Tests nur ausfuehren, wenn explizit angefordert.

## Konkrete Ausfuehrungsschritte

1. Bestehendes Domain-Modell, DbContext und API-Struktur analysieren.
2. Datenmodell erweitern und Fluent-Konfiguration inkl. Indizes erstellen.
3. Migration erzeugen und in Projekt einchecken.
4. Externe Metadaten-Clients (Open Library, Google Books) kapseln.
5. Fallback-Orchestrator-Service mit Resilience implementieren.
6. API-Endpunkt fuer ISBN-Scan bauen.
7. UI-Flow fuer Kamera-Scan + manuelle Eingabe integrieren.
8. Unit- und Integrations-Tests ergaenzen.
9. Build und relevante Tests ausfuehren.

## Akzeptanzkriterien

1. ISBN kann per Kamera-Flow (mit manueller Eingabe als Fallback) erfasst werden.
2. Metadaten werden ueber mindestens zwei kostenfreie APIs geladen.
3. Fallback-Strategie funktioniert nachweisbar.
4. Buch wird in der persoenlichen Bibliothek gespeichert.
5. Keine Duplikate fuer gleiche normalisierte ISBN.
6. EF-Core-Datenmodell und Migration sind implementiert und verifiziert.
7. Build erfolgreich, relevante Tests erfolgreich oder sauber mit Blocker dokumentiert.

## Erwartete Abschlussausgabe

Liefere am Ende:

1. Kurze Zusammenfassung der Aenderungen.
2. Liste aller geaenderten Dateien.
3. Verwendete externe APIs inkl. Reihenfolge (Primary/Fallback).
4. Datenmodell- und Migrationsueberblick.
5. Build-/Teststatus.
6. Kurzer Security-, UX- und Performance-Impact.

## Arbeitsmodus

Arbeite selbststaendig in kleinen, nachvollziehbaren Schritten und stoppe erst, wenn die Akzeptanzkriterien erfuellt sind oder ein echter Blocker mit Ursache vorliegt.
