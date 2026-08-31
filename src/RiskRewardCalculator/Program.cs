using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RiskRewardCalculator;
using RiskRewardCalculator.Services;

// This is the ONLY entry point of the whole application.
// When the browser downloads the WebAssembly payload, the runtime calls
// into this compiled Main() method exactly like a normal .NET console app.
// From here on, everything - routing, rendering, event handling - happens
// inside the browser tab, on the WebAssembly virtual CPU. No server is involved.
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// The root component "App" (App.razor) gets mounted into the <div id="app"> element
// declared in wwwroot/index.html.
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Standard ASP.NET Core dependency injection container - it works the same
// way here as it does in a server app, it just resolves services inside the browser.
// Calculators are stateless and pure, so they are safe to register as singletons.
builder.Services.AddSingleton<IPositionSizeCalculator, PositionSizeCalculator>();
builder.Services.AddSingleton<IRiskRewardCalculator, RiskRewardCalculatorService>();
builder.Services.AddSingleton<InstrumentPresetProvider>();
builder.Services.AddScoped<LocalStorageService>();

await builder.Build().RunAsync();
