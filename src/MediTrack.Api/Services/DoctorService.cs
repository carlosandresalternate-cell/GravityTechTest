using MediTrack.Api.Common;
using MediTrack.Api.Data;
using MediTrack.Api.DTOs;
using MediTrack.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MediTrack.Api.Services;

public class DoctorService : IDoctorService
{
    private readonly MediTrackDbContext _db;
    private readonly ILogger<DoctorService> _logger;
    private readonly IMemoryCache _cache;

    public DoctorService(MediTrackDbContext db, ILogger<DoctorService> logger, IMemoryCache cache)
    {
        _db = db;
        _logger = logger;
        _cache = cache;
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
        if (_cache.TryGetValue(Constants.AVAILABLE_DOCTORS_KEY, out List<DoctorWithAvailabilityResponse>? cached))
        {
            return cached!;
        }

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

        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        _cache.Set(Constants.AVAILABLE_DOCTORS_KEY, results, cacheEntryOptions);

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

        var availableDoctorsResponse = new List<DoctorResponse>();

        var activeDoctorsQuery = _db.Doctors
            .Where(d => d.IsActive)
            .Include(d => d.Specialty)
            .Include(d => d.Appointments)
            .OrderBy(d => d.LastName) // Sort by last name
            .ThenBy(d => d.FirstName) // Then by first name
            .AsQueryable();

        if (activeDoctorsQuery is null)
        {
            return availableDoctorsResponse; //no drs found
        }

        if (availableOn.HasValue)
        {
            availableDoctorsResponse = (await activeDoctorsQuery.ToListAsync())
                .Where(d => d.Appointments.Count(a => a.AppointmentDateTime.Date == availableOn.Value.Date) < 8
                && (specialty == null || d.Specialty!.Name.Contains(specialty, StringComparison.OrdinalIgnoreCase)))
                .Select(d => new DoctorResponse(
                    d.Id,
                    d.FullName,
                    d.Email,
                    d.Specialty != null ? string.IsNullOrWhiteSpace(d.Specialty.Name) ? "Unknown" : d.Specialty.Name : "Unknown",
                    d.IsActive
                )).ToList();
        }
        else
        {
            availableDoctorsResponse = (await activeDoctorsQuery.ToListAsync())
                .Where(d => specialty == null || d.Specialty!.Name.Contains(specialty, StringComparison.OrdinalIgnoreCase))
                .Select(d => new DoctorResponse(
                    d.Id,
                    d.FullName,
                    d.Email,
                    d.Specialty != null ? string.IsNullOrWhiteSpace(d.Specialty.Name) ? "Unknown" : d.Specialty.Name : "Unknown",
                    d.IsActive
                )).ToList();
        }

        return availableDoctorsResponse;
    }
}
