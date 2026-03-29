# Claude Master Prompt: Strikte Design-System-Anpassung fuer Bookly.Ui

Du bist ein erfahrener Blazor- und UX-Engineer. Implementiere im bestehenden Projekt `Bookly.Ui` ein konsistentes, produktionsreifes Design System auf Basis der UI-Prototypen im Ordner `artefacts/stitch_bookly`.

Wichtig: Arbeite mit hoher visueller Qualitaet, aber ohne "AI-template look". Fokus auf Accessibility, Responsiveness, Performance und klare Informationshierarchie.

## Ziel

Transformiere das aktuelle Standard-UI von `Bookly.Ui` in das visuelle System "Atelier" und verankere die Gestaltungsregeln als wiederverwendbare Tokens und Komponenten.

## Verbindliche Analysegrundlage

Analysiere vor jeder Umsetzung diese Dateien und leite daraus konkrete UI-Regeln ab:

1. `artefacts/stitch_bookly/lumina_library/DESIGN.md`
2. `artefacts/stitch_bookly/meine_bibliothek_saas_edition/code.html`
3. `artefacts/stitch_bookly/buch_details_saas_edition/code.html`
4. `artefacts/stitch_bookly/entdecken_saas_edition/code.html`
5. `artefacts/stitch_bookly/isbn_scanner_saas_edition/code.html`

Dokumentiere kurz, welche Designregeln aus welchen Artefakten stammen.

## Harte Vorgaben

1. Nur noetige Aenderungen in `Bookly.Ui` und zugehoerigen UI-E2E-Tests.
2. Kein visuelles Downgrade auf Standard-Bootstrap-Optik.
3. Keine harten, unstrukturierten Farben direkt in Komponenten, stattdessen Design Tokens.
4. Kein Border-Linien-Look als primaere Trennlogik (nutze Spacing, Flachen, Tonwerte).
5. Keine grossen ungefragten Refactorings ausserhalb der UI-Schicht.
6. Keine Secrets, keine sensiblen Daten in Logs oder UI.

## Design-System-Anforderungen (Pflicht)

### 1) Foundations / Tokens

Erzeuge oder erweitere zentrale Token fuer:

1. Farben (inkl. `primary`, `primary-container`, `surface`, `surface-container-*`, `tertiary`, `outline-variant`).
2. Typografie mit klarer Rollenverteilung:
- Headlines: Manrope
- Body/Labels: Inter
3. Spacing Scale (mind. 4, 8, 12, 16, 24, 32).
4. Radius Scale (mind. sm, md, lg, xl, full).
5. Shadow/Elevation Tokens mit weichen, diffusen Schatten.
6. Motion Tokens (Dauer, Easing) inkl. `prefers-reduced-motion` Ruecksicht.

### 2) Core Components

Baue oder style wiederverwendbare Komponenten/Styles fuer:

1. Top App Bar (frosted/glass, sticky).
2. Bottom Navigation (mobile-first, klarer aktiver Zustand).
3. Primary Button (Gradient), Secondary Button (Ghost), Destructive Action.
4. Search Input mit Icon-Slot und Focus-State.
5. Card-System (Standard, Glass, Featured, Metadata).
6. Status Chips/Tags (z. B. Featured, Borrowed, Completed).
7. Progress Bar im "Shelf"-Stil.
8. Toggle/Switch mit klaren Labels.
9. Empty, Loading, Error, Success States.

### 3) Seitenanpassung in Blazor

Wende das Design System mindestens auf die vorhandenen Kernseiten an:

1. Home/Library Experience (curated grid/list).
2. Add-Seite im neuen visuellen System.
3. Mindestens eine Detailansicht (Book Details) mit Metadatenkarte.

Wenn sinnvoll, ergaenze neue Blazor-Seiten fuer Discover/Scan, aber nur wenn sauber integrierbar.

## UX- und A11y-Anforderungen (Pflicht)

1. WCAG 2.2 AA als Ziel.
2. Vollstaendige Tastaturbedienbarkeit fuer Navigation, Formulare und interaktive Karten.
3. Deutlich sichtbare Focus States (nicht nur Farbaenderung).
4. Semantische HTML-Struktur und sinnvolle ARIA-Attribute.
5. Ausreichende Kontraste fuer Text, Controls und Status-Elemente.
6. Valide Form- und Fehlernachrichten nah am Feld.
7. Keine blockierenden Modals ohne zwingenden Grund.

## Performance-Anforderungen

1. Vermeide unnoetige JS-Abhaengigkeiten.
2. Nutze vorhandene Blazor/CSS-Mittel bevorzugt.
3. Halte CSS wartbar: Token-zentriert, keine massiven Duplikate.
4. Bilder responsiv und in passenden Groessen.
5. Animationen sparsam und zielgerichtet.

## Empfohlene technische Umsetzung

1. Lege globale Tokens zentral in `wwwroot/app.css` oder einer klar benannten Design-System-CSS an.
2. Strukturiere Komponentenstyles ueber `.razor.css` (wo sinnvoll).
3. Halte Seitenlogik getrennt von visueller Darstellung.
4. Entferne/ueberschreibe alte Template-Stile nur kontrolliert.

## Verifikation (verpflichtend)

Fuehre nach der Umsetzung mindestens aus:

1. `dotnet build`
2. Relevante Tests (`dotnet test` fuer vorhandene Unit/E2E Projekte)
3. UI-Checks:
- Navigation und Fokuspfade manuell pruefen
- Responsiveness (mobile + desktop)
- Kernzustande (loading/empty/error/success) sichtbar verifizieren

## Akzeptanzkriterien

1. Design Tokens sind zentral definiert und in Seiten/Komponenten aktiv genutzt.
2. UI orientiert sich klar am Atelier-Look der Artefakte.
3. Keine dominante Bootstrap-Standardoptik mehr.
4. Mindestens Home, Add und eine Detailansicht sind im neuen System umgesetzt.
5. Fokus, Kontrast und Tastaturbedienung sind sichtbar verbessert.
6. Build erfolgreich.
7. Relevante Tests erfolgreich oder Blocker sauber dokumentiert.

## Erwartete Abschlussausgabe

Liefere am Ende:

1. Kurze Zusammenfassung der Design-System-Aenderungen.
2. Liste aller geaenderten Dateien.
3. Welche Regeln aus welchen Artefakten uebernommen wurden.
4. Ergebnis von Build und Tests.
5. Kurzer A11y-, Security- und Performance-Impact.
6. Offene Punkte/Trade-offs.

## Arbeitsmodus

Arbeite in kleinen, nachvollziehbaren Schritten und stoppe erst, wenn die Akzeptanzkriterien erfuellt sind oder ein echter, klar benannter Blocker vorliegt.