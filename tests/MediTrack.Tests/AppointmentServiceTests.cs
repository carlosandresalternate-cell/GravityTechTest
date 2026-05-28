using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MediTrack.Api.Data;
using MediTrack.Api.DTOs;
using MediTrack.Api.Models;
using MediTrack.Api.Services;

namespace MediTrack.Tests;

/// <summary>
/// TODO (Task 4): Implement unit tests for AppointmentService.
/// 
/// Required test cases:
/// 
/// 1. BookAppointmentAsync_ShouldSucceed_WhenSlotIsAvailable
///    - Arrange: valid patient, valid doctor, no conflicting appointment
///    - Act: call BookAppointmentAsync
///    - Assert: appointment is created with correct data, status is Scheduled
///
/// 2. BookAppointmentAsync_ShouldThrow_WhenSlotAlreadyTaken
///    - Arrange: existing appointment at the same time for same doctor
///    - Act: call BookAppointmentAsync with same time/doctor
///    - Assert: throws InvalidOperationException
///
/// 3. BookAppointmentAsync_ShouldThrow_WhenDoctorNotFound
///    - Arrange: non-existent doctor ID
///    - Act: call BookAppointmentAsync
///    - Assert: throws ArgumentException
///
/// 4. BookAppointmentAsync_ShouldThrow_WhenPatientNotFound
///    - Arrange: non-existent patient ID
///    - Act: call BookAppointmentAsync
///    - Assert: throws ArgumentException
///
/// 5. CancelAppointmentAsync_ShouldSucceed_WhenAppointmentIsScheduled
///    - Arrange: appointment with Scheduled status
///    - Act: call CancelAppointmentAsync
///    - Assert: returns true, status is Cancelled
///
/// 6. CancelAppointmentAsync_ShouldThrow_WhenAppointmentIsCompleted
///    - (After fixing Bug #5)
///    - Arrange: appointment with Completed status
///    - Act: call CancelAppointmentAsync
///    - Assert: throws InvalidOperationException
/// </summary>
public class AppointmentServiceTests
{
    private MediTrackDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MediTrackDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new MediTrackDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    // Helper: seed a doctor for tests
    private async Task<Doctor> SeedDoctor(MediTrackDbContext db, int id = 100)
    {
        var specialty = new Specialty { Id = id, Name = "TestSpecialty" };
        db.Specialties.Add(specialty);

        var doctor = new Doctor
        {
            Id = id,
            FirstName = "Test",
            LastName = "Doctor",
            Email = "test@doctor.com",
            Phone = "555-000-0000",
            SpecialtyId = id,
            IsActive = true
        };
        db.Doctors.Add(doctor);
        await db.SaveChangesAsync();
        return doctor;
    }

    // Helper: seed a patient for tests
    private async Task<Patient> SeedPatient(MediTrackDbContext db, int id = 100)
    {
        var patient = new Patient
        {
            Id = id,
            FirstName = "Test",
            LastName = "Patient",
            Email = "test@patient.com",
            Phone = "555-000-0001",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsActive = true
        };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();
        return patient;
    }

    // TODO: Write your tests below. Use the helpers above.
    // Example structure:

    [Fact]
    public async Task BookAppointmentAsync_ShouldSucceed_WhenSlotIsAvailable()
    {
        // Arrange
        using var db = CreateInMemoryContext();
        var doctor = await SeedDoctor(db, id: 200);
        var patient = await SeedPatient(db, id: 200);

        var loggerMock = new Mock<ILogger<AppointmentService>>();
        var service = new AppointmentService(db, loggerMock.Object);

        var appointmentDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(9); // tomorrow at 09:00 UTC
        var request = new BookAppointmentRequest(
            patient.Id,
            doctor.Id,
            appointmentDateTime,
            "Routine check",
            "test notes"
        );

        // Act
        var result = await service.BookAppointmentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(patient.Id, result.PatientId);
        Assert.Equal(doctor.Id, result.DoctorId);
        Assert.Equal(appointmentDateTime, result.AppointmentDateTime);
        Assert.Equal("Scheduled", result.Status);

        // Verify persisted in DB
        var persisted = await db.Appointments.FirstOrDefaultAsync(a => a.Id == result.Id);
        Assert.NotNull(persisted);
        Assert.Equal(AppointmentStatus.Scheduled, persisted!.Status);
        Assert.Equal(patient.Id, persisted.PatientId);
        Assert.Equal(doctor.Id, persisted.DoctorId);
    }

    [Fact]
    public async Task BookAppointmentAsync_ShouldThrow_WhenSlotAlreadyTaken()
    {
        // Arrange
        using var db = CreateInMemoryContext();
        var doctor = await SeedDoctor(db, id: 201);
        var patient1 = await SeedPatient(db, id: 201);
        var patient2 = await SeedPatient(db, id: 202);

        var appointmentDateTime = DateTime.UtcNow.AddDays(2).Date.AddHours(10);

        // seed an existing scheduled appointment for the doctor at that time
        var existing = new Appointment
        {
            PatientId = patient1.Id,
            DoctorId = doctor.Id,
            AppointmentDateTime = appointmentDateTime,
            Reason = "Existing",
            Status = AppointmentStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };
        db.Appointments.Add(existing);
        await db.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<AppointmentService>>();
        var service = new AppointmentService(db, loggerMock.Object);

        // attempt to book another appointment for the same doctor at the same time with a different patient
        var request = new BookAppointmentRequest(
            patient2.Id,
            doctor.Id,
            appointmentDateTime,
            "New booking attempt",
            "test notes"
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.BookAppointmentAsync(request));
        Assert.Contains("This time slot is already booked for this doctor.", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BookAppointmentAsync_ShouldThrow_WhenDoctorNotFound()
    {
        // Arrange
        using var db = CreateInMemoryContext();
        var patient = await SeedPatient(db, id: 300);

        var loggerMock = new Mock<ILogger<AppointmentService>>();
        var service = new AppointmentService(db, loggerMock.Object);

        var missingDoctorId = 9999;
        var appointmentDateTime = DateTime.UtcNow.AddDays(3).Date.AddHours(11);
        var request = new BookAppointmentRequest(
            patient.Id,
            missingDoctorId,
            appointmentDateTime,
            "Attempt with missing doctor",
            "test notes"
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.BookAppointmentAsync(request));
        Assert.Contains($"Doctor with ID {missingDoctorId} not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BookAppointmentAsync_ShouldThrow_WhenPatientNotFound()
    {
        // Arrange
        using var db = CreateInMemoryContext();
        var doctor = await SeedDoctor(db, id: 400);

        var loggerMock = new Mock<ILogger<AppointmentService>>();
        var service = new AppointmentService(db, loggerMock.Object);

        var missingPatientId = 8888;
        var appointmentDateTime = DateTime.UtcNow.AddDays(4).Date.AddHours(14);
        var request = new BookAppointmentRequest(
            missingPatientId,
            doctor.Id,
            appointmentDateTime,
            "Attempt with missing patient",
            "test notes"
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.BookAppointmentAsync(request));
        Assert.Contains($"Patient with ID {missingPatientId} not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CancelAppointmentAsync_ShouldSucceed_WhenScheduled()
    {
        Assert.True(true, "TODO: Implement this test");
    }

    [Fact]
    public async Task CancelAppointmentAsync_ShouldThrow_WhenCompleted()
    {
        // NOTE: This test should pass AFTER fixing Bug #5
        Assert.True(true, "TODO: Implement this test");
    }
}
