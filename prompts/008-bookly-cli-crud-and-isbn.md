# Bookly CLI: CRUD-Operationen und ISBN-Auflösung

## Ziel
Implementiere eine funktionale CLI für die Bookly-Anwendung, die:
1. **CRUD-Operationen** für Bücher und Autoren mit Datenbankpersistenz
2. **ISBN-Auflösung** unter Nutzung bestehender API-Dienste
3. **Wiederverwendbare Kernfunktionalität** im Core-Projekt
4. **Produktionsreife Tests** für alle Funktionen

## State of the Art
- Entities (Book, Author, BookAuthor) existieren im Core
- BooklyDbContext mit PostgreSQL ist konfiguriert
- ISBN-Validator funktioniert
- API hat BookLookupOrchestrator Service
- Core.Tests und Cli-Projekt sind leer/minimal

## Anforderungen

### 1. Kernfunktionalität im Core-Projekt (`Bookly.Core`)

#### 1.1 Services und Repositories
Erstelle Domain Services und Repositories mit **striktem SRP**:

**`Bookly.Core/Services/IBookRepository.cs`** und Implementierung:
- `GetBookByIdAsync(int id)` → Book
- `GetBookByIsbnAsync(string normalizedIsbn)` → Book?
- `GetAllBooksAsync(skip, take)` → IEnumerable<Book> (mit Pagination)
- `CreateBookAsync(Book book)` → Book
- `UpdateBookAsync(Book book)` → Book
- `DeleteBookAsync(int id)` → bool

**`Bookly.Core/Services/IAuthorRepository.cs`** und Implementierung:
- `GetAuthorByIdAsync(int id)` → Author
- `GetAuthorByNameAsync(string name)` → Author?
- `GetAllAuthorsAsync(skip, take)` → IEnumerable<Author>
- `CreateAuthorAsync(Author author)` → Author
- `UpdateAuthorAsync(Author author)` → Author
- `DeleteAuthorAsync(int id)` → bool

**`Bookly.Core/Services/IIsbnMetadataService.cs`** und Implementierung:
- `ResolveIsbnAsync(string isbn)` → BookMetadata?
- Nutze bestehende IsbnValidator Logik
- Integriere mit API-Lookup (z.B. OpenLibrary, Google Books)
- **Fallback-Strategie** dokumentiert und getestet

**`Bookly.Core/UseCases/AddBookUseCase.cs`**:
- ISBN validieren via IsbnValidator
- Metadata via IsbnMetadataService auflösen (mit Fallbacks)
- Buch in DB speichern oder Duplikate erkennen (via NormalizedIsbn)
- Autoren Create-or-Get Pattern
- Transaktionale Integrität sicherstellen

#### 1.2 DTOs im Kern
Verschiebe relevante DTOs nach Core (oder definiere Core-DTOs):
- `AddBookRequest` (ISBN, optional ManualTitle, ManualAuthors)
- `BookDto` (komplettes Buch mit Authors)

#### 1.3 Typen Options & Validation
Definiere Configuration-Klassen mit Validation:
- `IsbnMetadataServiceOptions` (API-Keys, Timeouts, Retries)
- `DatabaseOptions` (ConnectionString, Pool Size, Migrations-Verhalten)

### 2. CLI-Projekt (`Bookly.Cli`)

#### 2.1 Struktur
```
Bookly.Cli/
├── Program.cs (Entry Point + DI)
├── Commands/
│   ├── ICommand.cs (Interface)
│   ├── AddBookCommand.cs
│   ├── ListBooksCommand.cs
│   ├── GetBookCommand.cs
│   ├── UpdateBookCommand.cs
│   ├── DeleteBookCommand.cs
│   ├── ListAuthorsCommand.cs
│   └── AuthorsCommand/
│       ├── GetAuthorCommand.cs
│       ├── AddAuthorCommand.cs
│       ├── UpdateAuthorCommand.cs
│       └── DeleteAuthorCommand.cs
├── Output/
│   └── CsvFormatter.cs, JsonFormatter.cs, etc.
└── Handlers/
    └── CommandHandler.cs (Router)
```

#### 2.2 Command Pattern & DI
- Jedes Command implementiert `ICommand { Task<int> ExecuteAsync(CancellationToken ct); }`
- `CommandHandler` routet Command Names zu Implementierungen
- Dependency Injection für Repositories, UseCase, Logger

#### 2.3 CLI-Befehle
Alle Befehle mit validiertem Exit-Code (0 = Erfolg, 1 = User Error, 2 = System Error):

**Buch-Befehle:**
```bash
# ISBN-basiert: Metadata auflösen + speichern
bookly book add-by-isbn <isbn>
  --title "Override Title" (optional)
  --author "Author Name" (optional, mehrfach)
  --output json|csv|table (default: table)

# Manuell ohne ISBN
bookly book add-manual <title>
  --author <author> (mehrfach, required ≥1)
  --subtitle <subtitle> (optional)
  --output json|csv|table

# Liste mit Pagination
bookly book list
  --skip <n> (default: 0)
  --take <n> (default: 10)
  --output json|csv|table

# Einzelnes Buch
bookly book get <id>
  --output json|detailed

# Aktualisieren
bookly book update <id>
  --title <neuer Titel> (optional)
  --subtitle <neuen Untertitel> (optional)
  --author <neue Autoren> (optional, überschreibt)
  --output json

# Löschen (mit Bestätigung)
bookly book delete <id>
  --force (ohne Bestätigung)
```

**Autoren-Befehle:**
```bash
# Autor hinzufügen
bookly author add <name>
  --output json

# Autor auflisten
bookly author list
  --skip <n> (default: 0)
  --take <n> (default: 10)
  --output json|table

# Einzelnen Autor abrufen
bookly author get <id>
  --output json

# Autor aktualisieren
bookly author update <id>
  --name <neuer Name>
  --output json

# Autor löschen (mit Bestätigung)
bookly author delete <id>
  --force (ohne Bestätigung)
```

#### 2.4 Fehlerbehandlung & Validierung
- Validierungsfehler sofort mit aussagekräftiger Nachricht => Exit 1
- DB-Fehler (Z-Constraints, Duplikate) => Exit 1 + strukturierte Nachricht
- Connection/Timeout-Fehler => Exit 2 + Hinweis auf --help
- Alle Exceptions loggen mit Context (Command, Input, Stack)

#### 2.5 Output-Formate
Unterstütze formatierte Ausgabe:
- **table** (ASCII-Tabelle, default)
- **json** (strukturiert)
- **csv** (für Import/Export)

### 3. Testing

#### 3.1 Unit Tests (`Bookly.Core.Tests`)
**Repositories:**
- `AddBookUseCase_ValidIsbn_ResolvesMetadata_SavesBook()` — Erfolgsfall
- `AddBookUseCase_InvalidIsbn_ThrowsValidationException()`
- `AddBookUseCase_DuplicateIsbn_UpdatesExisting()` oder Custom Exception
- `BookRepository_CreateBook_AssignsIdAndTimestamps()`
- `AuthorRepository_Get_Or_Create_Pattern()` — Author existiert bereits
- `AuthorRepository_Get_Or_Create_Pattern_New()` — Author ist neu

**ISBN Metadata Service:**
- `ResolveIsbn_ValidIsbn_FetchesMetadata()` — Mocked HTTP
- `ResolveIsbn_InvalidIsbn_ReturnsNull()`
- `ResolveIsbn_ApiTimeout_UsesFallback()` — Circuit Breaker Pattern
- `ResolveIsbn_MissingMetadata_UsesDefaults()`

**ISBN Validator (bestehend):**
- `IsbnValidator_ValidIsbn10_ReturnsValid()`
- `IsbnValidator_ValidIsbn13_ReturnsValid()`
- `IsbnValidator_InvalidChecksum_ReturnsInvalid()`

#### 3.2 Integration Tests (`Bookly.Cli`)
**Mit Testcontainers PostgreSQL:**
- `AddBookCommand_WithValidIsbn_CreatesBookInDb()`
- `AddBookCommand_WithInvalidIsbn_ExitCode1()`
- `ListBooksCommand_ReturnsBooks_FormattedAsJson()`
- `GetBookCommand_ExistingId_ReturnsBook()`
- `GetBookCommand_InvalidId_ExitCode1()`
- `UpdateBookCommand_ChangesTitle_PersistsChange()`
- `DeleteBookCommand_RemovesBook_VerifyNotExist()`
- `AddAuthorCommand_CreatesAuthor_InDb()`
- `AuthorRepository_CreateOrGet_Idempotent()`

**Transaktionale Tests:**
- `AddBook_With_NewAuthors_AllRollbackOnError()` — Rollback bei Fehler

#### 3.3 Test Data & Builders
Erstelle `TestData.cs` mit:
- `ValidIsbn13`, `ValidIsbn10`, `InvalidIsbn` Konstanten
- `SampleBookMetadata()` (mit relevante Fallback-Szenarios)
- `SampleAuthor()`, `SampleBook()` Builders
- Fixture-Klasse mit Testcontainers DB

### 4. Configuration & Startup

#### 4.1 CLI Program.cs
```csharp
// appsettings.Development.json nutzen
// DbContext registrieren mit DbContextPool
// Repositories als Scoped registrieren
// UseCase Services als Transient
// Logger konfigurieren
// IHostBuilder ("CLI Host") statt WebHost
```

#### 4.2 appsettings.Development.json (CLI)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Bookly": "Debug"
    }
  },
  "Bookly": {
    "Database": {
      "ConnectionString": "Host=localhost;Database=bookly_dev;..."
    },
    "IsbnMetadataService": {
      "Timeout": 5000,
      "MaxRetries": 2,
      "FallbackEnabled": true
    }
  }
}
```

### 5. Security & Performance

#### 5.1 Security
- Keine Secrets im Code (appsettings.Development.json ist für Local Dev, .gitignore!)
- SQL Injection: Nutze EF Core Parameterized Queries
- Input Trim & Normalize (ISBNs, Namen)
- Keine sensiblen Daten in Logs (keine ISBNs, keine User Data)

#### 5.2 Performance
- **Pagination immer** bei List-Commands (default 10)
- **Indizes** für Books(NormalizedIsbn), Authors(Name)
- **Connection Pooling** via DbContextPool
- **Lazy-Loading vermeiden**, eager-load via Include()
- ISBN-API Calls cacoen mit TTL (z.B. 24h für erfolg, 1h für Fehler)

### 6. Definition of Done
- [ ] Alle Repositories mit CRUD funktionieren
- [ ] IsbnMetadataService mit Fallback-Strategie
- [ ] AddBookUseCase mit Transaction-Handling
- [ ] Alle CLI-Commands implementiert & getestet
- [ ] Unit Tests > 85% Coverage für Core.Services
- [ ] Integration Tests mit Testcontainers laufen
- [ ] appsettings.Development.json gitignored
- [ ] README mit CLI-Beispiele aktualisiert
- [ ] Migrations aktuell (add, list, update Änderungen)
- [ ] AGENTS.MD Policy: Core-Services sind wiederverwendbar, CLI ist nur Consumer

## Priorität
1. **Repository Layer & Domain Services** (Core)
2. **UseCase AddBook** (ISBN Validation → Fetch Metadata → Create-or-Update)
3. **Integration Tests** (Testcontainers)
4. **CLI Commands** (book add-by-isbn, list, get, delete, update)
5. **Author Commands** (CRUD)
6. **Output Formatters** (JSON, CSV, Table)
7. **Observability** (Structured Logging, Performance Metrics)

## Architektur-Prinzipien
- **Core ist unabhängig von CLI**: Core kennt nur ENTITIES, SERVICES, USE-CASES. Keine CLI-spezifischen Referenzen.
- **Services sind Singletons** (isbnMetadataService, Logger Factory)
- **Repositories sind Scoped** (DbContext-Lebenszyklus)
- **Commands sind Command Pattern**: Jedes Command ist testbar, kann sein Input validieren.
- **Errors sind strukturiert**: ValidationException, RepositoryException (mit Codes).

## Referenzen
- AGENTS.MD "Testing Policy", "API-Standards", "Coding Standards"
- Bestehender IsbnValidator, BookLookupOrchestrator
- EF Core Best Practices: DbContext Pooling, Migrations, Indexes
- CLI Frameworks: System.CommandLine (Microsoft) oder minimal Args-Parsing mit Guard Clauses
