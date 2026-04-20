using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using WinForge.Web;
using WinForge.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API Http Client
var apiAddress = builder.Configuration["ApiBaseAddress"] ?? "http://localhost:5195";
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiAddress)
});

// MudBlazor
builder.Services.AddMudServices();

// App services
builder.Services.AddScoped<IAuthStateService, AuthStateService>();
builder.Services.AddScoped<IApiClient, ApiClient>();

await builder.Build().RunAsync();
