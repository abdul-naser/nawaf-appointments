using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NawafAppointment;
using NawafAppointment.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<AppSettingsService>();
builder.Services.AddScoped<PageHeaderService>();
builder.Services.AddScoped<ReminderService>();

await builder.Build().RunAsync();
