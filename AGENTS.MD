# AGENTS.MD

## Zweck
Diese Datei definiert verbindliche Regeln fuer AI- und Human-Agents, die an dieser .NET Aspire Applikation arbeiten. Ziel ist konsistent hochqualitativer Code mit starkem Fokus auf UI, UX, Security, Performance, Wartbarkeit und operativer Exzellenz.

## Geltungsbereich
Diese Regeln gelten fuer das gesamte Repository, inklusive AppHost, ServiceDefaults, API-Services, Worker, Frontend, IaC und Tests.

## Core Principles
1. Liefere nur produktionsreife Aenderungen, keine Demo-Loesungen.
2. Behalte bestehende Architektur- und Naming-Konventionen bei.
3. Schreibe kleine, reversible, testbare Aenderungen.
4. Bevorzuge Klarheit vor Cleverness.
5. Security und Performance sind Default, keine spaete Optimierung.
6. Jede Aenderung braucht passende Tests und Telemetrie.

## Aspire- und Architektur-Leitlinien
1. Nutze .NET Aspire Patterns fuer Service Discovery, Health Checks, Resilience und Observability.
2. Konfiguriere Services ueber typed options mit Validation beim Startup.
3. Setze Timeouts, Retries, Circuit Breaker und Bulkhead dort, wo externe Abhaengigkeiten genutzt werden.
4. Definiere fuer jeden Service:
- Liveness und Readiness Checks.
- Structured Logging mit Correlation/Trace IDs.
- OpenTelemetry Traces, Metrics und relevante Business Events.
5. Halte Netzwerkgrenzen explizit:
- Keine direkten DB-Zugriffe aus UI.
- Kommunikation nur ueber klar definierte APIs/Messaging.
6. Bevorzuge asynchrone IO-Endpunkte und cancellation tokens durchgaengig.

## UI und UX Standards
1. Accessibility zuerst:
- WCAG 2.2 AA als Ziel.
- Vollstaendige Keyboard-Navigation.
- Sichtbare Focus-States.
- Semantisches HTML und korrekte ARIA-Nutzung.
2. Responsive by default:
- Mobile-first Layouts.
- Breakpoints konsistent und komponentenbasiert.
3. UX fuer reale Nutzung:
- Klare Loading-, Empty-, Error- und Success-States.
- Sinnvolle Validierungsnachrichten nah am Eingabefeld.
- Vermeide blockierende Modals ohne zwingenden Grund.
4. Performance im Frontend:
- Reduziere JS-Bundle-Groesse.
- Lazy-Loading fuer schwere Komponenten.
- Bilder in passenden Formaten und Groessen.
5. Konsistente Design Tokens fuer Farbe, Abstand, Typografie, Radius, Schatten.

## Security Baseline (Nicht verhandelbar)
1. Keine Secrets im Code, in Tests oder in Logs.
2. Secrets nur aus sicheren Quellen (z. B. Key Vault, lokale Secret Stores fuer Dev).
3. Strikte Input-Validierung an allen API-Grenzen.
4. Output-Encoding und Schutz gegen XSS, Injection, CSRF (wo relevant).
5. AuthN/AuthZ muss explizit sein:
- Default-Deny fuer sensible Endpunkte.
- Rollen/Policies statt ad-hoc Checks.
6. Nutze HTTPS/TLS erzwingend, sichere Header und CORS minimal noetig.
7. Error Responses duerfen keine internen Details preisgeben.
8. Dependency-Updates regelmaessig, bekannte CVEs zeitnah beheben.

## Performance Baseline
1. Definiere Performance-Ziele pro kritischem Endpoint (z. B. p95 Latenz, Throughput).
2. Vermeide N+1 Queries, unnoetige Objektallokationen und sync-over-async.
3. Nutze Caching bewusst:
- Klare TTLs.
- Invalidation-Strategie dokumentiert.
4. Pagination/Filtering/Projection fuer Listenendpunkte ist Pflicht.
5. Nutze Streaming fuer grosse Payloads, wenn sinnvoll.
6. Miss vor und nach Optimierungen (Profiling statt Vermutung).

## API- und Domain-Standards
1. API Contracts sind stabil und versionierbar.
2. DTOs trennen von Domain-Modellen.
3. Fehlerformat konsistent (z. B. RFC 7807 ProblemDetails).
4. Idempotenz fuer geeignete Schreiboperationen.
5. Domain-Regeln gehoeren in Domain/Application Layer, nicht in Controller/UI.

## Daten und Persistenz
1. Migrationen sind deterministisch und rollback-faehig.
2. Indizes entlang realer Query-Patterns pflegen.
3. Transaktionsgrenzen bewusst setzen.
4. PII klassifizieren und nur minimal noetig speichern.
5. Retention- und Loeschkonzepte beruecksichtigen.

## Testing Policy
1. Test-Pyramide einhalten:
- Unit Tests fuer Logik und Edge Cases.
- Integrations-Tests fuer DB, Messaging, externe APIs (mit Testcontainern/Mocks wo sinnvoll).
- End-to-End fuer kritische User-Flows.
2. Neue Features und Bugfixes ohne Tests sind unvollstaendig.
3. Flaky Tests sind sofort zu stabilisieren oder temporaer zu quarantainen mit Ticket.
4. Sicherheitskritische und performancekritische Pfade explizit testen.

## Observability und Betrieb
1. Jeder Service emittiert:
- Strukturierte Logs mit Severity und Kontext.
- Traces ueber Servicegrenzen.
- Kernmetriken (Requests, Fehler, Latenz, Saettigung/Queue).
2. Dashboards und Alerts fuer kritische SLOs pflegen.
3. Deployment muss health-basiert validiert werden (Smoke Checks).
4. Feature Flags fuer riskante Rollouts nutzen.

## Coding Standards (C#/.NET)
1. Nullable Reference Types aktiv und korrekt nutzen.
2. Nutze guard clauses und fruehe exits fuer lesbaren Kontrollfluss.
3. Exceptions nur fuer exceptional cases, nicht fuer normale Control Flows.
4. Oeffentliche APIs, Methoden und Klassen klar benennen.
5. Kommentare nur fuer "warum", nicht fuer offensichtliches "was".
6. Vermeide statische globale Zustaende; bevorzuge DI.
7. Halte Methoden kurz und single-responsibility orientiert.

## Arbeitsmodus fuer Agents
1. Vor jeder Aenderung:
- Relevante Dateien, Architekturgrenzen und Risiken identifizieren.
- Annahmen explizit machen.
2. Bei Implementierung:
- Kleine Commits/Schritte.
- Keine ungefragten grossen Refactorings.
- Bestehenden Stil einhalten.
3. Nach Implementierung:
- Build, Tests, Lint/Analyzer und relevante Checks laufen lassen.
- Security- und Performance-Auswirkungen kurz dokumentieren.
4. Bei Unsicherheit:
- Sicherste Option waehlen.
- Trade-offs transparent festhalten.

## Pull Request / Change Checklist
1. Problem und Ziel klar beschrieben.
2. Architekturentscheidung nachvollziehbar.
3. Tests hinzugefuegt/aktualisiert und lokal erfolgreich.
4. Keine Secrets, keine sensiblen Daten in Logs.
5. Observability fuer neue Pfade vorhanden.
6. Performance-Risiken bewertet und bei Bedarf gemessen.
7. UX/A11y fuer UI-Aenderungen geprueft.
8. Rollback- oder Mitigationsstrategie vorhanden.

## Definition of Done
Eine Aenderung ist erst fertig, wenn:
1. Code, Tests und Doku aktualisiert sind.
2. Security-, Performance- und UX-Implikationen geprueft sind.
3. Telemetrie fuer Betrieb und Debugging vorhanden ist.
4. Reviewer die Aenderung ohne implizites Wissen verstehen koennen.

## Prioritaet bei Konflikten
Bei Zielkonflikten gilt folgende Reihenfolge:
1. Security und Datenintegritaet.
2. Korrektheit und Zuverlaessigkeit.
3. UX und Accessibility.
4. Performance und Kosten.
5. Entwicklungsaufwand.
