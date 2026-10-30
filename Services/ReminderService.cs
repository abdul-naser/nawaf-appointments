using Microsoft.JSInterop;
using MudBlazor;
using NawafAppointment.Models;

namespace NawafAppointment.Services;

public class ReminderService : IDisposable
{
    private readonly AppointmentService _appointments;
    private readonly AppSettingsService _settings;
    private readonly IJSRuntime _js;
    private Timer? _timer;
    private readonly HashSet<string> _notifiedIds = [];

    public event Func<string, Severity, Task>? OnReminder;

    public ReminderService(
        AppointmentService appointments,
        AppSettingsService settings,
        IJSRuntime js)
    {
        _appointments = appointments;
        _settings = settings;
        _js = js;
    }

    public void Start()
    {
        _timer?.Dispose();
        _timer = new Timer(_ => _ = CheckRemindersAsync(), null,
            TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    private async Task CheckRemindersAsync()
    {
        if (!_settings.Settings.RemindersEnabled) return;

        var upcoming = _appointments.Appointments
            .Where(a => a.GetStatus() == AppointmentStatus.Upcoming && a.Date.HasValue)
            .ToList();

        foreach (var app in upcoming)
        {
            if (string.IsNullOrEmpty(app.Id) || _notifiedIds.Contains(app.Id)) continue;

            var remaining = app.TimeUntil();
            if (!remaining.HasValue) continue;

            var hours = remaining.Value.TotalHours;

            if (hours <= 24 && hours > 23)
            {
                _notifiedIds.Add(app.Id);
                await NotifyAsync(
                    $"⏰ موعد نواف غداً: {app.DoctorName} — {app.HospitalName}",
                    Severity.Info);
            }
            else if (hours <= 1 && hours > 0)
            {
                _notifiedIds.Add(app.Id);
                await NotifyAsync(
                    $"🔔 موعد نواف خلال ساعة: {app.DoctorName} — {app.Date:hh:mm tt}",
                    Severity.Warning);
            }
        }
    }

    private async Task NotifyAsync(string message, Severity severity)
    {
        try
        {
            await _js.InvokeVoidAsync("showBrowserNotification", message);
        }
        catch { /* optional */ }

        if (OnReminder is not null)
            await OnReminder(message, severity);
    }

    public void Dispose() => _timer?.Dispose();
}
