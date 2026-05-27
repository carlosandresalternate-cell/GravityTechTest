using Microsoft.EntityFrameworkCore;
using MediTrack.Api.Data;
using MediTrack.Api.DTOs;
using MediTrack.Api.Models;

namespace MediTrack.Api.Services;

public class PatientService : IPatientService
{
    private readonly MediTrackDbContext _db;
    private readonly ILogger<PatientService> _logger;

    public PatientService(MediTrackDbContext db, ILogger<PatientService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResponse<PatientResponse>> GetPatientsAsync(int page, int pageSize)
    {
        var totalCount = await _db.Patients.Where(p => p.IsActive).CountAsync();

        var patients = await _db.Patients
            .Where(p => p.IsActive)
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = patients.Select(p => new PatientResponse(
            p.Id, p.FullName, p.Email, p.Phone, p.DateOfBirth, p.Address, p.IsActive
        )).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResponse<PatientResponse>(items, totalCount, page, pageSize, totalPages);
    }

    /// <summary>
    /// Gets a patient by ID with their appointment history.
    /// 
    /// *** BUG #3: MISSING NULL CHECK ***
    /// This method doesn't properly handle the case where the patient doesn't exist.
    /// It will throw a NullReferenceException instead of returning null gracefully.
    /// </summary>
    public async Task<PatientDetailResponse?> GetPatientByIdAsync(int id)
    {
        var patient = await _db.Patients
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.Specialty)
            .FirstOrDefaultAsync(p => p.Id == id);

        // BUG #3: No null check — directly accessing patient properties
        // If patient is null, this will throw NullReferenceException
        var appointments = patient.Appointments
            .OrderByDescending(a => a.AppointmentDateTime)
            .Select(a => new AppointmentResponse(
                a.Id,
                a.PatientId,
                patient.FullName,
                a.DoctorId,
                a.Doctor?.FullName ?? "Unknown",
                a.Doctor?.Specialty?.Name ?? "Unknown",
                a.AppointmentDateTime,
                a.OriginalDateTime,
                a.Reason,
                a.Notes,
                a.Status.ToString()
            ))
            .ToList();

        return new PatientDetailResponse(
            patient.Id,
            patient.FullName,
            patient.Email,
            patient.Phone,
            patient.DateOfBirth,
            patient.Address,
            patient.IsActive,
            appointments
        );
    }

    public async Task<PatientResponse> CreatePatientAsync(CreatePatientRequest request)
    {
        var patient = new Patient
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created patient {PatientId} - {PatientName}", patient.Id, patient.FullName);

        return new PatientResponse(
            patient.Id, patient.FullName, patient.Email, patient.Phone,
            patient.DateOfBirth, patient.Address, patient.IsActive
        );
    }

    public async Task<PatientResponse?> UpdatePatientAsync(int id, UpdatePatientRequest request)
    {
        var patient = await _db.Patients.FindAsync(id);
        if (patient == null) return null;

        patient.FirstName = request.FirstName;
        patient.LastName = request.LastName;
        patient.Email = request.Email;
        patient.Phone = request.Phone;
        patient.Address = request.Address;

        await _db.SaveChangesAsync();

        return new PatientResponse(
            patient.Id, patient.FullName, patient.Email, patient.Phone,
            patient.DateOfBirth, patient.Address, patient.IsActive
        );
    }

    public async Task<bool> DeletePatientAsync(int id)
    {
        var patient = await _db.Patients.FindAsync(id);
        if (patient == null) return false;

        // Soft delete
        patient.IsActive = false;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Soft-deleted patient {PatientId}", id);
        return true;
    }
}
