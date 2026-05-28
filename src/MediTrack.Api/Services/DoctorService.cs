using Microsoft.EntityFrameworkCore;
using MediTrack.Api.Data;
using MediTrack.Api.DTOs;
using MediTrack.Api.Models;

namespace MediTrack.Api.Services;

public class DoctorService : IDoctorService
{
    private readonly MediTrackDbContext _db;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(MediTrackDbContext db, ILogger<DoctorService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<DoctorResponse>> GetAllDoctorsAsync()
    {
        var doctors = await _db.Doctors
            .Include(d => d.Specialty)
            .Where(d => d.IsActive)
            .ToListAsync();

        return doctors.Select(d => new DoctorResponse(
            d.Id,
            d.FullName,
            d.Email,
            d.Specialty?.Name ?? "Unknown",
            d.IsActive
        )).ToList();
    }

    public async Task<DoctorResponse?> GetDoctorByIdAsync(int id)
    {
        var doctor = await _db.Doctors
            .Include(d => d.Specialty)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor == null) return null;

        return new DoctorResponse(
            doctor.Id,
            doctor.FullName,
            doctor.Email,
            doctor.Specialty?.Name ?? "Unknown",
            doctor.IsActive
        );
    }

    /// <summary>
    /// Gets all active doctors with their upcoming appointment counts.
    /// 
    /// *** BUG #1: N+1 QUERY PROBLEM ***
    /// This method loads all doctors, then for EACH doctor makes a separate DB call
    /// to count appointments. For 100 doctors, this makes 101 queries.
    /// 
    /// FIX: Use eager loading with .Include() or a projection query to load
    /// everything in a single round-trip.
    /// </summary>
    public async Task<List<DoctorWithAvailabilityResponse>> GetDoctorsWithAvailabilityAsync()
    {
        // BUG #1: N+1 query — loads doctors, then queries appointments one-by-one
        //fix: use Include to load appointments in the same query
        var doctors = await _db.Doctors
            .Where(d => d.IsActive)
            .Include(d => d.Appointments)
            .Include(d => d.Specialty)
            .ToListAsync();

        var results = new List<DoctorWithAvailabilityResponse>();

        results = doctors.Select(doctor =>
        {
            var upcomingCount = doctor.Appointments
                .Count(a => a.AppointmentDateTime > DateTime.UtcNow && a.Status == AppointmentStatus.Scheduled);
            // Log the count (still in hot path, but at least not a separate query)
            _logger.LogInformation($"Doctor {doctor.FullName} has {upcomingCount} upcoming appointments");
            return new DoctorWithAvailabilityResponse(
                doctor.Id,
                doctor.FullName,
                doctor.Email,
                doctor.Specialty?.Name ?? "Unknown",
                upcomingCount,
                new List<DateTime>() // Simplified: not calculating actual slots
            );
        }).ToList();

        return results;
    }

    /// <summary>
    /// Search doctors by specialty and/or availability on a given date.
    /// TODO: Implement this method (Task 2b)
    /// </summary>
    public async Task<List<DoctorResponse>> SearchDoctorsAsync(string? specialty, DateTime? availableOn)
    {
        // TODO: Implement search logic
        // - Filter by specialty name (case-insensitive partial match)
        // - If availableOn is provided, only return doctors who have available slots that day
        //   (a doctor is "available" if they have fewer than 8 appointments on that day)
        // - Sort by last name, then first name
        // - Only return active doctors
        throw new NotImplementedException("Task 2b: Implement doctor search");
    }
}
