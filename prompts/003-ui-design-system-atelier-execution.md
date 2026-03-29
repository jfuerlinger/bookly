# Claude Execution Prompt: Atelier Design System Rollout (Kurz)

Du bist ein Senior Blazor UI Engineer. Setze jetzt das Atelier Design System in `Bookly.Ui` um.

## Kontext

Nutze als Quelle:

1. `prompts/002-ui-design-system-atelier-strict.md`
2. `artefacts/stitch_bookly/lumina_library/DESIGN.md`
3. `artefacts/stitch_bookly/*/code.html`

## Auftrag

Modernisiere die bestehende Blazor-UI von Template-Optik zu Atelier-Optik (premium, editorial, mobile-first) mit Fokus auf Reuse, A11y und Performance.

## Ausfuehrungsreihenfolge (verbindlich)

1. Ist-Analyse in `Bookly.Ui`:
- vorhandene Layouts, Seiten, CSS-Dateien erfassen
- alte Bootstrap-Styles identifizieren, die ersetzt werden muessen

2. Foundations bauen:
- zentrale Tokens in `wwwroot/app.css` oder separater Design-System-CSS
- Farben, Typografie (Manrope/Inter), Spacing, Radius, Elevation, Motion
- sichtbare Focus-Styles und `prefers-reduced-motion` ergaenzen

3. Shell-Komponenten umsetzen:
- Top App Bar (glass/frosted)
- Bottom Navigation (mobile-first, klarer active state)

4. Core-UI-Komponenten stylen:
- Buttons (Primary Gradient, Secondary Ghost, Destructive)
- Input/Search Field
- Cards (Standard/Featured/Glass/Metadata)
- Chips/Tags, Toggle, Shelf-Progress

5. Seiten migrieren (mindestens):
- `Home.razor` als curated library view
- `Add.razor` im neuen Design
- eine Book-Detailansicht (neu oder bestehend erweitern)

6. States absichern:
- loading, empty, error, success fuer zentrale Flows sichtbar implementieren

7. Cleanup:
- redundante Alt-Styles entfernen
- visuelle Konsistenz und Lesbarkeit sicherstellen

8. Verifikation:
- `dotnet build`
- `dotnet test`
- manuell: Tastatur-Navigation, Focus-Sichtbarkeit, mobile + desktop Layout

## Harte Qualitaetskriterien

1. Keine dominante Bootstrap-Default-Anmutung.
2. Keine ungeordneten Hardcoded-Farbwerte in Komponenten.
3. Semantisches HTML und WCAG-orientierte Bedienbarkeit.
4. Keine unnötige JS-Komplexitaet.
5. Kleine, nachvollziehbare Aenderungsschritte.

## Abschlussformat

Liefere am Ende:

1. geaenderte Dateien
2. Build-/Test-Status
3. welche Artefakt-Regeln konkret uebernommen wurden
4. A11y- und Performance-Impact in 3-6 Stichpunkten

Arbeite selbststaendig bis fertig oder bis ein echter Blocker klar benannt ist.