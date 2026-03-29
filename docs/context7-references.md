# Context7-Referenzen

Dieses Dokument listet die Context7 Library-IDs, die bei der initialen Erstellung der Bookly-Lösung als Dokumentationsquelle verwendet wurden.

| Thema | Context7 Library-ID | Begründung |
|---|---|---|
| .NET Aspire | `/dotnet/aspire` | AppHost-Konfiguration, PostgreSQL-Integration (`AddPostgres`, `AddDatabase`, `WithReference`), Service Discovery für API/UI-Verdrahtung |
| Entity Framework Core | `/dotnet/entityframework.docs` | DbContext-Definition, Entity-Modellierung (`Book`), PostgreSQL-Provider-Konfiguration (`UseNpgsql`), Migrations-Setup |
| Blazor | `/dotnet/blazor-samples` | Blazor Web App Setup (`AddRazorComponents`, `AddInteractiveServerComponents`), Razor-Komponenten mit `@bind`, `@onclick`, `HttpClient`-Injection für API-Aufrufe |

## Hinweise

- **Aspire** wurde primär über `/dotnet/aspire` referenziert (High Reputation, 2310 Code Snippets, Benchmark 79.41). Die Doku lieferte die korrekte Syntax für `AddPostgres().AddDatabase()` und `AddNpgsqlDbContext<T>()`.
- **Entity Framework Core** über `/dotnet/entityframework.docs` (High Reputation, 2967 Code Snippets, Benchmark 78.44). Genutzt für DbContext-Pattern, Entity-Definition und Provider-Konfiguration.
- **Blazor** über `/dotnet/blazor-samples` (High Reputation, 336 Code Snippets, Benchmark 68.9). Genutzt für Interactive Server Rendering Setup, Razor-Komponenten-Patterns und HttpClient-Integration.
