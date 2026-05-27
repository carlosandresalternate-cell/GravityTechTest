using MediTrack.Api.DTOs;
using MediTrack.Api.Models;

namespace MediTrack.Api.Services;

/// <summary>
/// Service interface for doctor operations.
/// </summary>
public interface IDoctorService
{
    Task<List<DoctorResponse>> GetAllDoctorsAsync();
    Task<DoctorResponse?> GetDoctorByIdAsync(int id);
    Task<List<DoctorWithAvailabilityResponse>> GetDoctorsWithAvailabilityAsync();
    Task<List<DoctorResponse>> SearchDoctorsAsync(string? specialty, DateTime? availableOn);
}

/// <summary>
/// Service interface for patient operations.
/// </summary>
public interface IPatientService
{
    Task<PagedResponse<PatientResponse>> GetPatientsAsync(int page, int pageSize);
    Task<PatientDetailResponse?> GetPatientByIdAsync(int id);
    Task<PatientResponse> CreatePatientAsync(CreatePatientRequest request);
    Task<PatientResponse?> UpdatePatientAsync(int id, UpdatePatientRequest request);
    Task<bool> DeletePatientAsync(int id);
}

/// <summary>
/// Service interface for appointment operations.
/// </summary>
public interface IAppointmentService
{
    Task<List<AppointmentResponse>> GetUpcomingAppointmentsAsync(int? doctorId = null);
    Task<AppointmentResponse?> GetAppointmentByIdAsync(int id);
    Task<AppointmentResponse> BookAppointmentAsync(BookAppointmentRequest request);
    Task<bool> CancelAppointmentAsync(int appointmentId);
    Task<AppointmentResponse?> RescheduleAppointmentAsync(int appointmentId, RescheduleAppointmentRequest request);
}
