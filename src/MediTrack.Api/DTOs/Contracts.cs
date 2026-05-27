namespace MediTrack.Api.DTOs;

// ──── Request DTOs ────

public record CreatePatientRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateTime DateOfBirth,
    string? Address);

public record UpdatePatientRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Address);

public record BookAppointmentRequest(
    int PatientId,
    int DoctorId,
    DateTime AppointmentDateTime,
    string Reason,
    string? Notes);

public record RescheduleAppointmentRequest(
    DateTime NewDateTime);

// ──── Response DTOs ────

public record DoctorResponse(
    int Id,
    string FullName,
    string Email,
    string Specialty,
    bool IsActive);

public record DoctorWithAvailabilityResponse(
    int Id,
    string FullName,
    string Email,
    string Specialty,
    int UpcomingAppointmentCount,
    List<DateTime> NextAvailableSlots);

public record PatientResponse(
    int Id,
    string FullName,
    string Email,
    string Phone,
    DateTime DateOfBirth,
    string? Address,
    bool IsActive);

public record PatientDetailResponse(
    int Id,
    string FullName,
    string Email,
    string Phone,
    DateTime DateOfBirth,
    string? Address,
    bool IsActive,
    List<AppointmentResponse> Appointments);

public record AppointmentResponse(
    int Id,
    int PatientId,
    string PatientName,
    int DoctorId,
    string DoctorName,
    string Specialty,
    DateTime AppointmentDateTime,
    DateTime? OriginalDateTime,
    string Reason,
    string? Notes,
    string Status);

public record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
