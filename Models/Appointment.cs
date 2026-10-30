using System.Text.Json.Serialization;

namespace NawafAppointment.Models;

public enum AppointmentStatus
{
    Upcoming,
    Completed,
    Cancelled
}

public class Appointment
{
    public string? Id { get; set; }
    public string? DoctorName { get; set; }
    public string? HospitalName { get; set; }
    public string? Specialty { get; set; }
    public DateTime? Date { get; set; }
    public string? Note { get; set; }
    public bool IsCancelled { get; set; }

    [JsonIgnore]
    public TimeSpan? Time { get; set; }

    public AppointmentStatus GetStatus()
    {
        if (IsCancelled) return AppointmentStatus.Cancelled;
        if (!Date.HasValue) return AppointmentStatus.Upcoming;
        return Date.Value < DateTime.Now ? AppointmentStatus.Completed : AppointmentStatus.Upcoming;
    }

    public string GetStatusLabel() => GetStatus() switch
    {
        AppointmentStatus.Upcoming => "قادم",
        AppointmentStatus.Completed => "منتهي",
        AppointmentStatus.Cancelled => "ملغي",
        _ => ""
    };

    public string GetArabicDayName()
    {
        if (!Date.HasValue) return "";
        return Date.Value.DayOfWeek switch
        {
            DayOfWeek.Saturday => "السبت",
            DayOfWeek.Sunday => "الأحد",
            DayOfWeek.Monday => "الاثنين",
            DayOfWeek.Tuesday => "الثلاثاء",
            DayOfWeek.Wednesday => "الأربعاء",
            DayOfWeek.Thursday => "الخميس",
            DayOfWeek.Friday => "الجمعة",
            _ => ""
        };
    }

    public TimeSpan? TimeUntil()
    {
        if (!Date.HasValue || GetStatus() != AppointmentStatus.Upcoming) return null;
        return Date.Value - DateTime.Now;
    }

    public bool IsWithin24Hours()
    {
        var remaining = TimeUntil();
        return remaining.HasValue && remaining.Value.TotalHours <= 24 && remaining.Value.TotalHours > 0;
    }
}
