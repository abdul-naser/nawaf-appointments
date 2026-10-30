using Microsoft.JSInterop;
using NawafAppointment.Models;

namespace NawafAppointment.Services;

public class AppSettingsService
{
    private readonly IJSRuntime _js;
    public event Action? OnChange;
    public AppSettings Settings { get; private set; } = new();

    public AppSettingsService(IJSRuntime js) => _js = js;

    public async Task LoadAsync()
    {
        try
        {
            var saved = await _js.InvokeAsync<AppSettings?>("loadSettings");
            Settings = saved ?? new AppSettings();
            NotifyStateChanged();
        }
        catch
        {
            Settings = new AppSettings();
        }
    }

    public async Task SetRemindersEnabled(bool enabled)
    {
        Settings.RemindersEnabled = enabled;
        await SaveAsync();
    }

    public async Task SetDarkMode(bool dark)
    {
        Settings.DarkMode = dark;
        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        await _js.InvokeVoidAsync("saveSettings", Settings);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
