using MediTrack.Api.Common;
using MediTrack.Api.Data;
using MediTrack.Api.DTOs;
using MediTrack.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MediTrack.Api.Services;

public class AppointmentService : IAppointmentService
{
    private readonly MediTrackDbContext _db;
    private readonly ILogger<AppointmentService> _logger;
    private readonly IMemoryCache _cache;

    public AppointmentService(MediTrackDbContext db, ILogger<AppointmentService> logger, IMemoryCache cache)
    {
        _db = db;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Gets upcoming appointments, optionally filtered by doctor.
    /// 
    /// *** BUG #2: WRONG DATE FILTER ***
    /// The filter uses &lt; instead of &gt; for the date comparison,
    /// so it returns PAST appointments instead of UPCOMING ones.
    /// </summary>
    public async Task<List<AppointmentResponse>> GetUpcomingAppointmentsAsync(int? doctorId = null)
    {
        // BUG #2: Wrong comparison operator — should be > (greater than) not < (less than)
        var query = _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Specialty)
            .Where(a => a.AppointmentDateTime > DateTime.UtcNow
                     && a.Status == AppointmentStatus.Scheduled);

        if (doctorId.HasValue)
        {
            query = query.Where(a => a.DoctorId == doctorId.Value);
        }

        var appointments = await query
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync();

        // BUG #6 (partial): String interpolation in logging
        _logger.LogInformation("Found {Count} upcoming appointments at {Time}", appointments.Count, DateTime.UtcNow);

        return appointments.Select(MapToResponse).ToList();
    }

    public async Task<AppointmentResponse?> GetAppointmentByIdAsync(int id)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Specialty)
            .FirstOrDefaultAsync(a => a.Id == id);

        return appointment == null ? null : MapToResponse(appointment);
    }

    /// <summary>
    /// Books a new appointment.
    /// 
    /// *** BUG #4: RACE CONDITION (DOUBLE-BOOKING) ***
    /// There's a check-then-act race condition: two concurrent requests could both
    /// pass the availability check and both insert an appointment at the same time slot.
    /// 
    /// FIX: Use a database-level unique constraint or pessimistic locking,
    /// or wrap the check+insert in a serializable transaction.
    /// </summary>
    public async Task<AppointmentResponse> BookAppointmentAsync(BookAppointmentRequest request)
    {
        // Use a serializable transaction so the check + insert are atomic.
        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);


        // BUG #6 (partial): String interpolation in logging
        _logger.LogInformation($"Booking appointment for patient {request.PatientId} with doctor {request.DoctorId} at {request.AppointmentDateTime}");

        // Verify doctor exists
        var doctor = await _db.Doctors
            .Include(d => d.Specialty)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId);

        if (doctor == null)
            throw new ArgumentException($"Doctor with ID {request.DoctorId} not found");

        // Verify patient exists
        var patient = await _db.Patients
            .FirstOrDefaultAsync(p => p.Id == request.PatientId);

        if (patient == null)
            throw new ArgumentException($"Patient with ID {request.PatientId} not found");

        // BUG #4: Race condition — this check and the insert below are NOT atomic.
        // Two requests arriving simultaneously can both pass this check.
        // fix: Availability check inside the transaction
        var conflicting = await _db.Appointments
            .AnyAsync(a => a.DoctorId == request.DoctorId
                        && a.AppointmentDateTime == request.AppointmentDateTime
                        && a.Status == AppointmentStatus.Scheduled);

        if (conflicting)
            throw new InvalidOperationException("This time slot is already booked for this doctor.");

        // No lock or transaction — another request could insert between the check above and here
        var appointment = new Appointment
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            AppointmentDateTime = request.AppointmentDateTime,
            Reason = request.Reason,
            Notes = request.Notes,
            Status = AppointmentStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        await transaction.CommitAsync();

        // Invalidate doctor availability cache — a new appointment changes upcoming counts
        _cache.Remove(Constants.AVAILABLE_DOCTORS_KEY);

        _logger.LogInformation($"Appointment {appointment.Id} booked successfully");

        return MapToResponse(appointment);
    }

    /// <summary>
    /// Cancels an appointment.
    /// 
    /// *** BUG #5: WRONG STATUS TRANSITION ***
    /// This method allows cancelling appointments that are already Completed.
    /// Only Scheduled appointments should be cancellable.
    /// </summary>
    public async Task<bool> CancelAppointmentAsync(int appointmentId)
    {
        var appointment = await _db.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
            return false;

        // BUG #5: Only checks for Cancelled status, but should also prevent
        // cancelling Completed and NoShow appointments.
        // Only Scheduled appointments should be cancellable.
        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            throw new InvalidOperationException("Appointment is already cancelled.");
        }

        if (appointment.Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidOperationException("Only scheduled appointments can be cancelled.");
        }

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Invalidate doctor availability cache — a cancelled appointment changes upcoming counts
        _cache.Remove(Constants.AVAILABLE_DOCTORS_KEY);

        _logger.LogInformation($"Appointment {appointmentId} cancelled");
        return true;
    }

    /// <summary>
    /// Reschedules an appointment to a new date/time.
    /// TODO: Implement this method (Task 2c)
    /// </summary>
    public async Task<AppointmentResponse?> RescheduleAppointmentAsync(
        int appointmentId, RescheduleAppointmentRequest request)
    {
        // TODO: Implement reschedule logic
        // - Find the appointment
        // - Verify it's in Scheduled status
        // - Check the new slot is available (no double-booking)
        // - Save the original DateTime to OriginalDateTime
        // - Update AppointmentDateTime to the new time
        // - Set UpdatedAt
        // - Return the updated appointment

        // Make the check + update atomic to avoid double-booking races
        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        // Load the appointment
        var appointment = await _db.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
            return null;

        // Only scheduled appointments can be rescheduled
        if (appointment.Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException("Appointment is not in Scheduled status and cannot be rescheduled.");

        // Check that the new slot is not already taken by another scheduled appointment for the same doctor
        var conflict = await _db.Appointments
            .AnyAsync(a => a.DoctorId == appointment.DoctorId
                        && a.AppointmentDateTime == request.NewDateTime
                        && a.Status == AppointmentStatus.Scheduled
                        && a.Id != appointmentId);

        if (conflict)
            throw new InvalidOperationException("The new time slot is already booked for this doctor.");

        // Preserve the original date/time (only set if not already set)
        if (!appointment.OriginalDateTime.HasValue)
        {
            appointment.OriginalDateTime = appointment.AppointmentDateTime;
        }

        var previousDateTime = appointment.AppointmentDateTime;
        appointment.AppointmentDateTime = request.NewDateTime;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        // Reload with navigation properties for the response mapping
        var updated = await _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Specialty)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        _logger.LogInformation("Appointment {Id} rescheduled from {Old} to {New}", appointmentId, previousDateTime, appointment.AppointmentDateTime);

        return updated == null ? null : MapToResponse(updated);
    }

    private static AppointmentResponse MapToResponse(Appointment a)
    {
        return new AppointmentResponse(
            a.Id,
            a.PatientId,
            a.Patient?.FullName ?? "Unknown",
            a.DoctorId,
            a.Doctor?.FullName ?? "Unknown",
            a.Doctor?.Specialty?.Name ?? "Unknown",
            a.AppointmentDateTime,
            a.OriginalDateTime,
            a.Reason,
            a.Notes,
            a.Status.ToString()
        );
    }
}
