namespace MediTrack.Api.Models;

using MediTrack.Api.Common;
using System.Text.Json.Serialization;

public class Doctor
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int SpecialtyId { get; set; }
    public Specialty? Specialty { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public List<Appointment> Appointments { get; set; } = new();
}

public class Patient
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [JsonIgnore] // To prevent circular reference during serialization
    public List<Appointment> Appointments { get; set; } = new();
}

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    
    [JsonIgnore] // To prevent circular reference during serialization
    public Patient? Patient { get; set; }
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public DateTime? OriginalDateTime { get; set; }  // For rescheduled appointments
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Concurrency token for optimistic concurrency (prevents double-booking races)
    public byte[] RowVersion { get; set; } = Utils.GenerateRandomRowVersion();
}

public enum AppointmentStatus
{
    Scheduled = 0,
    Completed = 1,
    Cancelled = 2,
    NoShow = 3
}

public class Specialty
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation property
    public List<Doctor> Doctors { get; set; } = new();
}
