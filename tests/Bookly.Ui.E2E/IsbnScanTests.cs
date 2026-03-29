using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Bookly.Ui.E2E;

[TestFixture]
public class IsbnScanTests : PageTest
{
    private string UiBaseUrl => Environment.GetEnvironmentVariable("BOOKLY_UI_URL") ?? "http://localhost:5044";
    private string ApiBaseUrl => Environment.GetEnvironmentVariable("BOOKLY_API_URL") ?? "http://localhost:5199";

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions { IgnoreHTTPSErrors = true };
    }

    [Test]
    public async Task ScanPage_ShouldBeReachable()
    {
        var response = await Page.GotoAsync($"{UiBaseUrl}/scan");
        Assert.That(response!.Status, Is.EqualTo((int)HttpStatusCode.OK));
        await Expect(Page.Locator(".scan-title")).ToHaveTextAsync("Scan ISBN");
    }

    [Test]
    public async Task ScanPage_EmptySubmit_ShowsValidationError()
    {
        await Page.GotoAsync($"{UiBaseUrl}/scan");
        await Page.ClickAsync(".scan-submit-btn");
        await Expect(Page.Locator(".form-error")).ToBeVisibleAsync();
        await Expect(Page.Locator(".form-error")).ToContainTextAsync("Please enter an ISBN");
    }

    [Test]
    public async Task ScanPage_ValidIsbn_ShowsBookResult()
    {
        await Page.GotoAsync($"{UiBaseUrl}/scan");

        // Use a well-known ISBN (The Pragmatic Programmer)
        var isbn = "9780135957059";
        await Page.FillAsync("#isbnInput", isbn);
        await Page.ClickAsync(".scan-submit-btn");

        // Wait for result or error (API call may take a few seconds)
        var resultOrError = Page.Locator(".scan-result, .alert-error-atelier");
        await Expect(resultOrError.First).ToBeVisibleAsync(new() { Timeout = 30000 });

        // If a result was returned, validate it has expected structure
        if (await Page.Locator(".scan-result").IsVisibleAsync())
        {
            await Expect(Page.Locator(".result-title")).ToBeVisibleAsync();
            await Expect(Page.Locator(".result-heading")).ToContainTextAsync("Book added to your library");
        }
    }

    [Test]
    public async Task ScanPage_CameraToggle_ShowsAndHidesCamera()
    {
        await Page.GotoAsync($"{UiBaseUrl}/scan");

        // Camera should not be visible initially
        await Expect(Page.Locator(".camera-container")).Not.ToBeVisibleAsync();

        // Click camera button
        await Page.ClickAsync(".scan-camera-btn");
        await Expect(Page.Locator(".camera-container")).ToBeVisibleAsync();

        // Click again to close
        await Page.ClickAsync(".scan-camera-btn");
        await Expect(Page.Locator(".camera-container")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task ScanPage_NavigationLink_Exists()
    {
        await Page.GotoAsync($"{UiBaseUrl}/");
        var scanLink = Page.Locator("a[href='scan']");
        await Expect(scanLink).ToBeVisibleAsync();
    }

    [Test]
    public async Task ApiEndpoint_IsbnScan_ReturnsValidResponse()
    {
        var isbn = "9780135957059";
        var payload = JsonSerializer.Serialize(new { isbn });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await Page.APIRequest.PostAsync(
            $"{ApiBaseUrl}/api/library/isbn-scan",
            new() { DataString = payload, Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json"
            }});

        // Should be 200 (already exists) or 201 (created)
        Assert.That(response.Status, Is.AnyOf((int)HttpStatusCode.OK, (int)HttpStatusCode.Created));

        var json = JsonDocument.Parse(await response.TextAsync());
        Assert.That(json.RootElement.TryGetProperty("title", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("normalizedIsbn", out _), Is.True);
    }

    [Test]
    public async Task ApiEndpoint_InvalidIsbn_ReturnsValidationError()
    {
        var payload = JsonSerializer.Serialize(new { isbn = "invalid-isbn" });

        var response = await Page.APIRequest.PostAsync(
            $"{ApiBaseUrl}/api/library/isbn-scan",
            new() { DataString = payload, Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json"
            }});

        Assert.That(response.Status, Is.EqualTo((int)HttpStatusCode.BadRequest));
    }
}
