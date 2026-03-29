using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Bookly.Ui.E2E;

[TestFixture]
public class ScalarDocsTests : PageTest
{
    private string ApiBaseUrl => Environment.GetEnvironmentVariable("BOOKLY_API_URL") ?? "http://localhost:5000";

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
        };
    }

    [Test]
    public async Task ScalarUI_ShouldBeReachable()
    {
        var response = await Page.GotoAsync($"{ApiBaseUrl}/docs");

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Status, Is.EqualTo((int)HttpStatusCode.OK));
        await Expect(Page).ToHaveTitleAsync(new Regex("Bookly API|Scalar"));
    }

    [Test]
    public async Task OpenApiJson_ShouldBeReachable()
    {
        var response = await Page.APIRequest.GetAsync($"{ApiBaseUrl}/openapi/v1.json");

        Assert.That(response.Status, Is.EqualTo((int)HttpStatusCode.OK));

        var body = await response.TextAsync();
        var json = JsonDocument.Parse(body);
        Assert.That(json.RootElement.TryGetProperty("openapi", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("paths", out _), Is.True);
    }

    [Test]
    public async Task ApiEndpoint_ShouldBeCallableViaOpenApi()
    {
        var response = await Page.APIRequest.GetAsync($"{ApiBaseUrl}/api/add?a=10&b=20");

        Assert.That(response.Status, Is.EqualTo((int)HttpStatusCode.OK));

        var body = await response.TextAsync();
        var json = JsonDocument.Parse(body);
        Assert.That(json.RootElement.GetProperty("result").GetInt32(), Is.EqualTo(30));
    }
}
