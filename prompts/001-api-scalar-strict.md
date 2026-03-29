# Claude Master Prompt: Strikte Scalar-Integration fuer Bookly.Api

Du bist ein erfahrener .NET- und API-Engineer. Implementiere im bestehenden Projekt `Bookly.Api` eine **strikte OpenAPI-Dokumentation mit Scalar**.

Wichtig: Es ist **keine Swagger-UI** erlaubt. Wenn Scalar nicht funktioniert, musst du die Ursache beheben, statt auf Swagger-UI auszuweichen.

## Ziel

Stelle sicher, dass ich API-Endpunkte im Browser ueber Scalar ansehen und direkt ausfuehren kann.

## Harte Vorgaben

1. Nur `Bookly.Api` anpassen, keine unnoetigen Architektur-Aenderungen.
2. OpenAPI-Dokument muss erzeugt und bereitgestellt werden.
3. Scalar muss als UI bereitgestellt werden.
4. **Swagger UI darf nicht konfiguriert oder aktiviert sein.**
5. Development-Umgebung:
- OpenAPI + Scalar sind aktiv.
6. Production-Umgebung:
- Nur aktivieren, wenn bereits eine explizite Projektkonvention dafuer existiert.
- Sonst auf Development beschraenken.
7. Security:
- Keine Secrets in Code/Logs.
- Keine internen technischen Details in Fehlerantworten exponieren.

## Konkrete Umsetzung

1. Analysiere zunaechst die bestehende `Program.cs` und vorhandene Paketreferenzen.
2. Ergaenze erforderliche NuGet-Pakete fuer:
- OpenAPI-Generierung
- Scalar-UI
3. Konfiguriere in `Program.cs`:
- OpenAPI-Endpunkt (z. B. `/openapi/v1.json`)
- Scalar-UI unter `/docs`
4. Entferne bzw. vermeide jede `UseSwaggerUI`-Konfiguration.
5. Halte die Aenderungen klein, klar und nachvollziehbar.
6. Erstelle einen dedizierten E2E-Testfall, der mindestens Folgendes validiert:
- Scalar UI unter `/docs` ist erreichbar.
- OpenAPI JSON-Endpunkt ist erreichbar.
- Mindestens ein API-Endpoint kann ueber die bereitgestellte API-Doku erfolgreich aufgerufen werden.
7. Fuer den E2E-Test ist **Playwright verpflichtend** (keine Alternative).
8. Nutze das vorhandene E2E-Testprojekt und lege dort einen klar benannten Test an (z. B. `ScalarDocsTests`).

## Verifikation (verpflichtend)

Fuehre nach der Implementierung mindestens aus:

1. Build:
- `dotnet build`
2. Relevante Tests:
- mindestens die bestehenden Unit-Tests, sofern im Workspace vorhanden
3. E2E-Testpflicht:
- Playwright-Test muss erstellt oder erweitert werden (kein Mock-only Test).
- den neu erstellten E2E-Test fuer die API-Dokumentation ausfuehren
- Test muss erfolgreich (gruen) durchlaufen
4. Laufzeitpruefung:
- API starten
- pruefen, dass `/docs` erreichbar ist
- pruefen, dass OpenAPI JSON-Endpunkt erreichbar ist

## Akzeptanzkriterien

1. Scalar ist unter `/docs` aufrufbar.
2. OpenAPI JSON ist aufrufbar.
3. Kein Swagger-UI-Endpunkt aktiv.
4. Mindestens ein Endpoint kann ueber Scalar ausgefuehrt werden.
5. Build ist erfolgreich.
6. Relevante Tests laufen erfolgreich oder begruendet dokumentiert, falls blockiert.
7. Ein neuer E2E-Test fuer die API-Dokumentation existiert und laeuft erfolgreich durch.
8. Der E2E-Test ist als Playwright-Test implementiert.

## Erwartete Abschlussausgabe

Liefere am Ende:

1. Kurze Zusammenfassung der Aenderungen.
2. Liste aller geaenderten Dateien.
3. Liste neu installierter/aktualisierter Pakete.
4. Exakte URLs fuer:
- Scalar UI
- OpenAPI JSON
5. Ergebnis von Build und Tests.
6. Kurzer Security- und Performance-Impact-Hinweis.
7. E2E-Testnachweis mit:
- Name/Pfad des neuen E2E-Tests
- ausgefuehrter Testbefehl
- Ergebnis (bestanden/fehlgeschlagen) mit kurzer Einordnung
8. Hinweis auf verwendete Playwright-Version bzw. vorhandenes Playwright-Testprojekt.

## Arbeitsmodus

Arbeite selbststaendig und ende erst, wenn alle Akzeptanzkriterien erfuellt sind oder ein echter Blocker mit konkreter Ursache vorliegt.