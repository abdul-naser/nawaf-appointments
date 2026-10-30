using Microsoft.JSInterop;
using NawafAppointment.Models;

namespace NawafAppointment.Services;

public class AppointmentService
{
    private readonly IJSRuntime _js;
    public event Action? OnChange;

    public List<Appointment> Appointments { get; private set; } = [];

    public static readonly List<string> Hospitals =
    [
        "مستشفى صحار",
        "مستشفى السلطاني",
        "المركز الصحي صحار",
        "أخرى"
    ];

    public static readonly List<string> Specialties =
    [
        "عيادة الأطفال",
        "عيادة العيون",
        "عيادة الأنف والأذن",
        "عيادة القلب",
        "عيادة العظام",
        "عيادة الجلدية",
        "عيادة الأسنان",
        "أخرى"
    ];

    public AppointmentService(IJSRuntime js) => _js = js;

    public async Task LoadAsync()
    {
        try
        {
            var result = await _js.InvokeAsync<List<Appointment>>("loadFromFirebase");
            Appointments = result ?? [];

            foreach (var app in Appointments)
            {
                if (app.Date.HasValue)
                    app.Time = app.Date.Value.TimeOfDay;
            }

            Appointments = Appointments
                .OrderBy(a => a.GetStatus() == AppointmentStatus.Upcoming ? 0 : 1)
                .ThenBy(a => a.Date)
                .ToList();

            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Load error: {ex.Message}");
        }
    }

    public async Task SaveAsync(Appointment appointment)
    {
        if (appointment.Date.HasValue && appointment.Time.HasValue)
            appointment.Date = appointment.Date.Value.Date + appointment.Time.Value;

        await _js.InvokeVoidAsync("saveToFirebase", appointment);
        await LoadAsync();
    }

    public async Task DeleteAsync(string? id)
    {
        if (string.IsNullOrEmpty(id)) return;
        await _js.InvokeVoidAsync("deleteFromFirebase", id);
        await LoadAsync();
    }

    public async Task CancelAsync(Appointment appointment)
    {
        appointment.IsCancelled = true;
        await SaveAsync(appointment);
    }

    public bool HasConflict(Appointment appointment) =>
        Appointments.Any(a =>
            a.Id != appointment.Id &&
            !a.IsCancelled &&
            a.Date == appointment.Date);

    public Appointment? GetNextUpcoming() =>
        Appointments
            .Where(a => a.GetStatus() == AppointmentStatus.Upcoming)
            .OrderBy(a => a.Date)
            .FirstOrDefault();

    public int CountThisWeek()
    {
        var now = DateTime.Now;
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Saturday);
        if (now.DayOfWeek < DayOfWeek.Saturday)
            weekStart = weekStart.AddDays(-7);
        var weekEnd = weekStart.AddDays(7);
        return Appointments.Count(a =>
            a.Date.HasValue &&
            a.Date.Value >= weekStart &&
            a.Date.Value < weekEnd &&
            a.GetStatus() != AppointmentStatus.Cancelled);
    }

    public int CountThisMonth() =>
        Appointments.Count(a =>
            a.Date?.Month == DateTime.Now.Month &&
            a.Date?.Year == DateTime.Now.Year &&
            a.GetStatus() != AppointmentStatus.Cancelled);

    public IEnumerable<Appointment> GetUpcomingWithin24Hours() =>
        Appointments.Where(a => a.IsWithin24Hours());

    public IEnumerable<Appointment> GetRecent(int count = 5) =>
        Appointments
            .Where(a => a.GetStatus() != AppointmentStatus.Cancelled)
            .OrderByDescending(a => a.Date)
            .Take(count);

    private void NotifyStateChanged() => OnChange?.Invoke();
}
