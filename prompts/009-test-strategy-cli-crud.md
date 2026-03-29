# Unit & Integration Test Strategy für Bookly CLI

## Ziel
Documenting konkrete Test-Szenarien mit Erwartungswerten und technischen Details für:
1. **Core Layer** (Services, Repositories, Use Cases)
2. **CLI Layer** (Command Execution, Output Formatting)
3. **Integration** (Testcontainers + Real DB)

## Teststruktur und Naming

### Filestruktur
```
tests/Bookly.Core.Tests/
├── Services/
│   ├── BookRepositoryTests.cs
│   ├── AuthorRepositoryTests.cs
│   └── IsbnMetadataServiceTests.cs
├── UseCases/
│   └── AddBookUseCaseTests.cs
├── Isbn/
│   └── IsbnValidatorTests.cs (erweiterbar)
├── Fixtures/
│   └── TestData.cs
└── Builders/
    ├── BookBuilder.cs
    └── AuthorBuilder.cs

tests/Bookly.Cli/
├── Commands/
│   ├── AddBookCommandTests.cs
│   ├── ListBooksCommandTests.cs
│   ├── GetBookCommandTests.cs
│   ├── DeleteBookCommandTests.cs
│   └── AuthorCommandTests.cs
├── Fixtures/
│   └── PostgresqlFixture.cs (Testcontainers)
└── Output/
    └── FormatterTests.cs
```

## Unit Tests (Bookly.Core.Tests)

### 1. IsbnValidator Tests

```csharp
public class IsbnValidatorTests
{
    [Theory]
    [InlineData("978-0-306-40615-2")] // Valid ISBN-13
    [InlineData("9780306406158")] // Valid ISBN-13 without dashes
    public void Validate_ValidIsbn13_ReturnsValid(string isbn)
    {
        // Arrange & Act
        var result = IsbnValidator.Validate(isbn);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.Error);
        Assert.Equal("9780306406158", result.NormalizedIsbn);
    }

    [Theory]
    [InlineData("0-306-40615-2")] // Valid ISBN-10
    [InlineData("030640615X")] // Valid ISBN-10 with checksum X
    public void Validate_ValidIsbn10_ReturnsValid(string isbn)
    {
        // Arrange & Act
        var result = IsbnValidator.Validate(isbn);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.Error);
    }

    [Theory]
    [InlineData("123456789")] // Too short
    [InlineData("978-0-306-40615-3")] // Invalid checksum
    [InlineData("not-an-isbn")] // Non-numeric
    public void Validate_InvalidIsbn_ReturnsInvalid(string isbn)
    {
        // Arrange & Act
        var result = IsbnValidator.Validate(isbn);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Error);
    }
}
```

### 2. Repository Tests (Mocked DbContext)

```csharp
public class BookRepositoryTests : IAsyncLifetime
{
    private BooklyDbContext _dbContext = null!;
    private BookRepository _repository = null!;

    async Task IAsyncLifetime.InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseInMemoryDatabase(databaseName: $"bookly-test-{Guid.NewGuid()}")
            .Options;

        _dbContext = new BooklyDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _repository = new BookRepository(_dbContext);
    }

    [Fact]
    public async Task CreateBook_ValidBook_ReturnsBookWithId()
    {
        // Arrange
        var book = new Book
        {
            Title = "Test Book",
            NormalizedIsbn = "9780306406158",
            MetadataSource = "test",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateBookAsync(book);
        await _dbContext.SaveChangesAsync();

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal("Test Book", result.Title);
    }

    [Fact]
    public async Task GetBookByIsbn_ExistingIsbn_ReturnsBook()
    {
        // Arrange
        var book = new Book
        {
            Title = "Test Book",
            NormalizedIsbn = "9780306406158",
            MetadataSource = "test",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        await _repository.CreateBookAsync(book);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetBookByIsbnAsync("9780306406158");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Book", result.Title);
    }

    [Fact]
    public async Task GetBookByIsbn_NonExistentIsbn_ReturnsNull()
    {
        // Act
        var result = await _repository.GetBookByIsbnAsync("9999999999999");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteBook_ExistingId_RemoveBook()
    {
        // Arrange
        var book = new Book
        {
            Title = "Test Book",
            NormalizedIsbn = "9780306406158",
            MetadataSource = "test",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        var created = await _repository.CreateBookAsync(book);
        await _dbContext.SaveChangesAsync();

        // Act
        var deleted = await _repository.DeleteBookAsync(created.Id);
        await _dbContext.SaveChangesAsync();

        // Assert
        Assert.True(deleted);
        var book2 = await _repository.GetBookByIdAsync(created.Id);
        Assert.Null(book2);
    }

    [Fact]
    public async Task GetAllBooks_Pagination_SkipAndTake()
    {
        // Arrange
        for (int i = 1; i <= 25; i++)
        {
            var book = new Book
            {
                Title = $"Book {i}",
                NormalizedIsbn = $"978030640615{i:2D}",
                MetadataSource = "test",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };
            await _repository.CreateBookAsync(book);
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var page1 = await _repository.GetAllBooksAsync(skip: 0, take: 10);
        var page2 = await _repository.GetAllBooksAsync(skip: 10, take: 10);

        // Assert
        Assert.Equal(10, page1.Count());
        Assert.Equal(10, page2.Count());
        Assert.NotEqual(page1.First().Id, page2.First().Id);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }
}
```

### 3. IsbnMetadataService Tests (Mocked HTTP)

```csharp
public class IsbnMetadataServiceTests
{
    [Fact]
    public async Task ResolveIsbn_ValidIsbn_FetchesMetadata()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var response = new BookMetadata
        {
            Isbn13 = "9780306406158",
            Title = "Test Book",
            Authors = ["Author One"],
            MetadataSource = "mock"
        };

        httpClientMock
            .Setup(x => x.GetAsync(It.Is<string>(u => u.Contains("9780306406158")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(response))
            });

        var service = new IsbnMetadataService(httpClientMock.Object);

        // Act
        var result = await service.ResolveIsbnAsync("978-0-306-40615-8");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Book", result.Title);
        Assert.Contains("Author One", result.Authors);
    }

    [Fact]
    public async Task ResolveIsbn_ApiTimeout_UsesFallback()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        httpClientMock
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Timeout"));

        var service = new IsbnMetadataService(httpClientMock.Object, enableFallback: true);

        // Act
        var result = await service.ResolveIsbnAsync("978-0-306-40615-8");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("[Fallback] Unknown Book", result.Title);
        Assert.Equal("fallback", result.Source);
    }

    [Theory]
    [InlineData("not-an-isbn")]
    [InlineData("123")]
    public async Task ResolveIsbn_InvalidIsbn_ReturnsNull(string isbn)
    {
        // Arrange
        var service = new IsbnMetadataService(new HttpClient());

        // Act
        var result = await service.ResolveIsbnAsync(isbn);

        // Assert
        Assert.Null(result);
    }
}
```

### 4. AddBookUseCase Tests

```csharp
public class AddBookUseCaseTests : IAsyncLifetime
{
    private BooklyDbContext _dbContext = null!;
    private BookRepository _bookRepository = null!;
    private AuthorRepository _authorRepository = null!;
    private IsbnMetadataService _metadataService = null!;
    private AddBookUseCase _useCase = null!;

    async Task IAsyncLifetime.InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseInMemoryDatabase(databaseName: $"bookly-test-{Guid.NewGuid()}")
            .Options;

        _dbContext = new BooklyDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        
        _bookRepository = new BookRepository(_dbContext);
        _authorRepository = new AuthorRepository(_dbContext);
        
        var httpClientMock = new Mock<HttpClient>();
        _metadataService = new IsbnMetadataService(httpClientMock.Object, enableFallback: true);
        
        _useCase = new AddBookUseCase(_bookRepository, _authorRepository, _metadataService);
    }

    [Fact]
    public async Task ExecuteAsync_ValidIsbn_CreatesBook()
    {
        // Arrange
        var isbn = "978-0-306-40615-8";

        // Act
        var result = await _useCase.ExecuteAsync(isbn, cancellationToken: CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        var stored = await _bookRepository.GetBookByIsbnAsync(IsbnValidator.Validate(isbn).NormalizedIsbn!);
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidIsbn_ThrowsValidationException()
    {
        // Arrange
        var isbn = "not-an-isbn";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync(isbn, cancellationToken: CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateIsbn_UpdatesExisting()
    {
        // Arrange
        var isbn = "978-0-306-40615-8";
        var normalized = IsbnValidator.Validate(isbn).NormalizedIsbn!;
        
        var firstBook = new Book
        {
            Title = "Original Title",
            NormalizedIsbn = normalized,
            MetadataSource = "test",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        var created1 = await _bookRepository.CreateBookAsync(firstBook);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _useCase.ExecuteAsync(isbn, cancellationToken: CancellationToken.None);

        // Assert
        Assert.Equal(created1.Id, result.Id); // Same ID
        var updated = await _bookRepository.GetBookByIdAsync(result.Id);
        Assert.NotEqual("Original Title", updated!.Title);
    }

    [Fact]
    public async Task ExecuteAsync_WithManualAuthors_CreatesOrGetAuthors()
    {
        // Arrange
        var isbn = "978-0-306-40615-8";
        var manualAuthors = new[] { "Author One", "Author Two" };

        // Act
        var result = await _useCase.ExecuteAsync(isbn, authorOverrides: manualAuthors, cancellationToken: CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var authors = result.BookAuthors.Select(ba => ba.Author!.Name).ToList();
        Assert.Contains("Author One", authors);
        Assert.Contains("Author Two", authors);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }
}
```

## Integration Tests (Bookly.Cli)

### Fixtures mit Testcontainers

```csharp
public class PostgresqlFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    public string? ConnectionString { get; private set; }

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithCleanUp(true)
            .WithImage("postgres:latest")
            .WithDatabase("bookly_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        // Apply migrations
        var options = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        using var dbContext = new BooklyDbContext(options);
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }
}

public class PostgresqlFixture_Collection : ICollectionFixture<PostgresqlFixture>
{
    // Collection definition
}
```

### CLI Command Integration Tests

```csharp
[Collection(nameof(PostgresqlFixture_Collection))]
public class AddBookCommandTests
{
    private readonly PostgresqlFixture _fixture;

    public AddBookCommandTests(PostgresqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddBookCommand_ValidIsbn_CreatesBookInDb()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        var dbContext = new BooklyDbContext(options);
        var repository = new BookRepository(dbContext);
        var httpClientMock = new Mock<HttpClient>();
        var metadataService = new IsbnMetadataService(httpClientMock.Object, enableFallback: true);
        var useCase = new AddBookUseCase(repository, new AuthorRepository(dbContext), metadataService);

        var command = new AddBookCommand(useCase);

        // Act
        var args = new[] { "add", "978-0-306-40615-8" };
        var exitCode = await command.ExecuteAsync(args, CancellationToken.None);

        // Assert
        Assert.Equal(0, exitCode);
        var book = await repository.GetBookByIsbnAsync("9780306406158");
        Assert.NotNull(book);
    }

    [Fact]
    public async Task AddBookCommand_InvalidIsbn_ExitCode1()
    {
        // Arrange
        var mockRepository = new Mock<IBookRepository>();
        var mockUseCase = new Mock<IAddBookUseCase>();
        var command = new AddBookCommand(mockUseCase.Object);

        // Act
        var args = new[] { "add", "not-an-isbn" };
        var exitCode = await command.ExecuteAsync(args, CancellationToken.None);

        // Assert
        Assert.Equal(1, exitCode);
    }
}

[Collection(nameof(PostgresqlFixture_Collection))]
public class ListBooksCommandTests
{
    private readonly PostgresqlFixture _fixture;

    public ListBooksCommandTests(PostgresqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ListBooksCommand_ReturnsBooks_FormattedAsJson()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        var dbContext = new BooklyDbContext(options);
        var repository = new BookRepository(dbContext);

        // Seed data
        var book = new Book
        {
            Title = "Test Book",
            NormalizedIsbn = "9780306406158",
            MetadataSource = "test",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        await repository.CreateBookAsync(book);
        await dbContext.SaveChangesAsync();

        var command = new ListBooksCommand(repository, outputFormat: "json");
        var output = new StringBuilder();

        // Act
        var exitCode = await command.ExecuteAsync(new[] { "list", "--output", "json" }, CancellationToken.None);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.NotEmpty(output.ToString());
        Assert.Contains("Test Book", output.ToString());
    }
}

[Collection(nameof(PostgresqlFixture_Collection))]
public class DeleteBookCommandTests
{
    private readonly PostgresqlFixture _fixture;

    public DeleteBookCommandTests(PostgresqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DeleteBookCommand_ExistingBook_RemovesAndExitCode0()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BooklyDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        var dbContext = new BooklyDbContext(options);
        var repository = new BookRepository(dbContext);

        var book = new Book
        {
            Title = "To Delete",
            NormalizedIsbn = "9781234567890",
            MetadataSource = "test",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        var created = await repository.CreateBookAsync(book);
        await dbContext.SaveChangesAsync();

        var command = new DeleteBookCommand(repository);

        // Act
        var exitCode = await command.ExecuteAsync(new[] { "delete", created.Id.ToString(), "--force" }, CancellationToken.None);

        // Assert
        Assert.Equal(0, exitCode);
        var deleted = await repository.GetBookByIdAsync(created.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteBookCommand_InvalidId_ExitCode1()
    {
        // Arrange
        var mockRepository = new Mock<IBookRepository>();
        mockRepository
            .Setup(x => x.GetBookByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Book)null!);

        var command = new DeleteBookCommand(mockRepository.Object);

        // Act
        var exitCode = await command.ExecuteAsync(new[] { "delete", "99999", "--force" }, CancellationToken.None);

        // Assert
        Assert.Equal(1, exitCode);
    }
}
```

## Output Formatter Tests

```csharp
public class JsonFormatterTests
{
    [Fact]
    public void Format_SingleBook_ReturnsValidJson()
    {
        // Arrange
        var book = new Book
        {
            Id = 1,
            Title = "Test Book",
            NormalizedIsbn = "9780306406158",
            MetadataSource = "test",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var formatter = new JsonFormatter();

        // Act
        var json = formatter.Format(new[] { book });

        // Assert
        Assert.NotEmpty(json);
        Assert.Contains("\"Title\":\"Test Book\"", json);
        Assert.Contains("9780306406158", json);
    }
}

public class TableFormatterTests
{
    [Fact]
    public void Format_Books_ReturnsAsciiTable()
    {
        // Arrange
        var book = new Book
        {
            Id = 1,
            Title = "Test Book",
            NormalizedIsbn = "9780306406158"
        };

        var formatter = new TableFormatter();

        // Act
        var table = formatter.Format(new[] { book });

        // Assert
        Assert.Contains("Test Book", table);
        Assert.Contains("9780306406158", table);
        Assert.Contains("─", table); // Table border
    }
}
```

## Test Konventionen

### Naming
- `{Method}_{Scenario}_{Expected}` (AAA Pattern)
- Beispiel: `AddBookCommand_ValidIsbn_CreatesBookInDb`

### Exit Codes
- **0**: Erfolg
- **1**: User-Fehler (ungültige Eingabe, Entity nicht gefunden)
- **2**: System-Fehler (DB-Fehler, Timeout, etc.)

### Assertions
- Nutze `Assert.*` aus xUnit
- Custom Assertions für komplexe Szenarien

### Mocking
- HttpClient mit Moq mocken
- DbContext mit InMemoryDatabase oder Testcontainers
- Use Domain-Events oder Notifications für Verifikation

### Cleanup
- `IAsyncLifetime` für async Setup/Teardown
- Testcontainers automatisch bereinigen

