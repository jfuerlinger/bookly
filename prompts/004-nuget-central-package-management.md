# Claude Master Prompt: NuGet Central Package Management einrichten

Du bist ein erfahrener .NET-Engineer. Migriere die bestehende Bookly-Solution auf **NuGet Central Package Management (CPM)**.

Wichtig: Es gibt keine Ausnahmen. Nach der Migration darf keine einzige `.csproj`-Datei mehr ein `Version`-Attribut an einem `<PackageReference>`-Element tragen.

## Ziel

Eine zentrale `Directory.Packages.props` im Solution-Root verwaltet alle Paketversionen. Jedes Projekt referenziert Pakete nur noch per Name, ohne Version.

## Harte Vorgaben

1. Alle Paketversionen landen in `Directory.Packages.props` an der Solution-Root.
2. Kein einziges `<PackageReference ... Version="...">` bleibt in einer `.csproj`.
3. Keine Wildcard-Versionen (`10.*`): alle Versionen exakt pinnen.
4. `PrivateAssets`, `IncludeAssets`, `ExcludeAssets` bleiben in den `.csproj`-Dateien.
5. Das Aspire AppHost SDK (`Sdk="Aspire.AppHost.Sdk/13.2.0"`) ist ein MSBuild-SDK, kein NuGet-Paket – es bleibt unveraendert im SDK-Attribut.
6. Versionskonflikte (gleiche Paketname, unterschiedliche Versionen) sind vor dem Eintrag aufzuloesen; die jeweils hoehere kompatible Version gewinnt, Ausnahme muss begruendet werden.
7. Nur `Bookly.Ui` und `Bookly.Cli` enthalten keine PackageReferences – das bleibt so.

## Ist-Zustand (Stand der Analyse)

Folgende Pakete und Versionen sind aktuell verteilt – inklusive bekannter Konflikte:

| Paket | Projekt | Aktuelle Version | Zielversion CPM |
|---|---|---|---|
| `Aspire.Hosting.PostgreSQL` | AppHost | 13.2.0 | 13.2.0 |
| `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` | Api | 13.2.0 | 13.2.0 |
| `Microsoft.AspNetCore.OpenApi` | Api | 10.0.0 | 10.0.0 |
| `Microsoft.EntityFrameworkCore` | Core | 10.0.5 | 10.0.5 |
| `Microsoft.EntityFrameworkCore.Design` | Api | **10.*** | **10.0.5** (Wildcard aufloesen) |
| `Microsoft.EntityFrameworkCore.Design` | Core | 10.0.5 | 10.0.5 |
| `Microsoft.Extensions.Http.Resilience` | ServiceDefaults | 10.1.0 | 10.1.0 |
| `Microsoft.Extensions.ServiceDiscovery` | ServiceDefaults | 10.1.0 | 10.1.0 |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Core | 10.0.1 | 10.0.1 |
| `OpenTelemetry.Exporter.OpenTelemetryProtocol` | ServiceDefaults | 1.14.0 | 1.14.0 |
| `OpenTelemetry.Extensions.Hosting` | ServiceDefaults | 1.14.0 | 1.14.0 |
| `OpenTelemetry.Instrumentation.AspNetCore` | ServiceDefaults | 1.14.0 | 1.14.0 |
| `OpenTelemetry.Instrumentation.Http` | ServiceDefaults | 1.14.0 | 1.14.0 |
| `OpenTelemetry.Instrumentation.Runtime` | ServiceDefaults | 1.14.0 | 1.14.0 |
| `Scalar.AspNetCore` | Api | 2.13.16 | 2.13.16 |
| `coverlet.collector` | Core.Tests, Ui.E2E | 6.0.4 | 6.0.4 |
| `Microsoft.NET.Test.Sdk` | Core.Tests | **17.14.1** | **17.14.1** |
| `Microsoft.NET.Test.Sdk` | Ui.E2E | **17.14.0** | **17.14.1** (Konflikt aufloesen) |
| `Microsoft.Playwright.NUnit` | Ui.E2E | 1.58.0 | 1.58.0 |
| `NUnit` | Ui.E2E | 4.3.2 | 4.3.2 |
| `NUnit.Analyzers` | Ui.E2E | 4.7.0 | 4.7.0 |
| `NUnit3TestAdapter` | Ui.E2E | 5.0.0 | 5.0.0 |
| `xunit` | Core.Tests | 2.9.3 | 2.9.3 |
| `xunit.runner.visualstudio` | Core.Tests | 3.1.4 | 3.1.4 |

## Konkrete Umsetzung

### Schritt 1: Directory.Packages.props anlegen

Lege `Directory.Packages.props` im Solution-Root (`/`) an:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- Aspire -->
    <PackageVersion Include="Aspire.Hosting.PostgreSQL" Version="13.2.0" />
    <PackageVersion Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" Version="13.2.0" />

    <!-- ASP.NET Core / OpenAPI -->
    <PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />
    <PackageVersion Include="Scalar.AspNetCore" Version="2.13.16" />

    <!-- Entity Framework Core -->
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.5" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5" />
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.1" />

    <!-- Extensions -->
    <PackageVersion Include="Microsoft.Extensions.Http.Resilience" Version="10.1.0" />
    <PackageVersion Include="Microsoft.Extensions.ServiceDiscovery" Version="10.1.0" />

    <!-- OpenTelemetry -->
    <PackageVersion Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.14.0" />
    <PackageVersion Include="OpenTelemetry.Extensions.Hosting" Version="1.14.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.14.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.Http" Version="1.14.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.Runtime" Version="1.14.0" />

    <!-- Test -->
    <PackageVersion Include="coverlet.collector" Version="6.0.4" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageVersion Include="Microsoft.Playwright.NUnit" Version="1.58.0" />
    <PackageVersion Include="NUnit" Version="4.3.2" />
    <PackageVersion Include="NUnit.Analyzers" Version="4.7.0" />
    <PackageVersion Include="NUnit3TestAdapter" Version="5.0.0" />
    <PackageVersion Include="xunit" Version="2.9.3" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>
</Project>
```

### Schritt 2: Version-Attribute aus allen .csproj entfernen

Bearbeite exakt diese Dateien:

- `src/Bookly.Api/Bookly.Api.csproj`
  - `Version="13.2.0"` bei `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` entfernen
  - `Version="10.0.0"` bei `Microsoft.AspNetCore.OpenApi` entfernen
  - `Version="10.*"` bei `Microsoft.EntityFrameworkCore.Design` entfernen (Wildcard weg)
  - `Version="2.13.16"` bei `Scalar.AspNetCore` entfernen

- `src/Bookly.Core/Bookly.Core.csproj`
  - `Version="10.0.5"` bei `Microsoft.EntityFrameworkCore` entfernen
  - `Version="10.0.5"` bei `Microsoft.EntityFrameworkCore.Design` entfernen
  - `Version="10.0.1"` bei `Npgsql.EntityFrameworkCore.PostgreSQL` entfernen

- `src/Bookly.AppHost/Bookly.AppHost.csproj`
  - `Version="13.2.0"` bei `Aspire.Hosting.PostgreSQL` entfernen
  - SDK-Attribut (`Sdk="Aspire.AppHost.Sdk/13.2.0"`) **unveraendert lassen**

- `src/Bookly.ServiceDefaults/Bookly.ServiceDefaults.csproj`
  - Alle `Version`-Attribute bei den sieben OpenTelemetry- und Extensions-Paketen entfernen

- `tests/Bookly.Core.Tests/Bookly.Core.Tests.csproj`
  - `Version`-Attribute bei `coverlet.collector`, `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio` entfernen

- `tests/Bookly.Ui.E2E/Bookly.Ui.E2E.csproj`
  - `Version`-Attribute bei allen fuenf Testpaketen entfernen

### Schritt 3: Verifikation

1. `dotnet build` auf Solution-Ebene – Build muss sauber durchlaufen.
2. `dotnet test` – alle Unit Tests muessen gruen sein.
3. Manuell pruefen: kein `<PackageReference ... Version="...">` in einer `.csproj` mehr vorhanden.
   - Schnellcheck: `grep -r 'PackageReference.*Version=' src/ tests/` muss leer zurueckkehren.

## Akzeptanzkriterien

1. `Directory.Packages.props` existiert im Solution-Root mit `ManagePackageVersionsCentrally=true`.
2. Keine `.csproj` enthaelt noch ein `Version`-Attribut in einem `<PackageReference>`.
3. Keine Wildcard-Versionen in `Directory.Packages.props`.
4. `dotnet build` erfolgreich.
5. Alle bestehenden Tests laufen erfolgreich durch.
6. `grep`-Pruefung bestaetigt versionsfreie PackageReferences.

## Erwartete Abschlussausgabe

1. Liste aller geaenderten Dateien.
2. Build- und Test-Status.
3. Ergebnis des `grep`-Checks.
4. Eventuelle Abweichungen vom Plan mit Begruendung.

## Arbeitsmodus

Starte sofort mit der Umsetzung. Erst stoppen, wenn alle Akzeptanzkriterien erfuellt oder ein echter Blocker benannt ist.
