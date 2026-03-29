using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Bookly.Ui.E2E;

[TestFixture]
public class AddPageTests : PageTest
{
    private string BaseUrl => Environment.GetEnvironmentVariable("BOOKLY_UI_URL") ?? "http://localhost:5002";

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
        };
    }

    private async Task FillNumber(string selector, string value)
    {
        var locator = Page.Locator(selector);
        await locator.ClickAsync();
        await locator.PressAsync("Control+a");
        await locator.PressAsync("Meta+a");
        await locator.FillAsync(value);
    }

    [Test]
    public async Task AddPage_ShouldDisplayResult_WhenNumbersAreEntered()
    {
        await Page.GotoAsync($"{BaseUrl}/add");

        await Expect(Page.Locator("h1")).ToHaveTextAsync("Addition");

        await FillNumber("#numberA", "3");
        await FillNumber("#numberB", "5");
        await Page.ClickAsync("#calculateBtn");

        await Expect(Page.Locator("#result")).ToBeVisibleAsync();
        await Expect(Page.Locator("#result")).ToContainTextAsync("8");
    }

    [Test]
    public async Task AddPage_ShouldHandleNegativeNumbers()
    {
        await Page.GotoAsync($"{BaseUrl}/add");

        await FillNumber("#numberA", "-10");
        await FillNumber("#numberB", "7");
        await Page.ClickAsync("#calculateBtn");

        await Expect(Page.Locator("#result")).ToBeVisibleAsync();
        await Expect(Page.Locator("#result")).ToContainTextAsync("-3");
    }

    [Test]
    public async Task AddPage_ShouldHandleZeros()
    {
        await Page.GotoAsync($"{BaseUrl}/add");

        await FillNumber("#numberA", "0");
        await FillNumber("#numberB", "0");
        await Page.ClickAsync("#calculateBtn");

        await Expect(Page.Locator("#result")).ToBeVisibleAsync();
        await Expect(Page.Locator("#result")).ToContainTextAsync("0");
    }
}
