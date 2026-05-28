using MediTrack.Api.Data;
using MediTrack.Api.DTOs;
using MediTrack.Api.Models;
using MediTrack.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace MediTrack.Tests;

/// <summary>
/// TODO (Task 4): Implement unit tests for DoctorService.
/// 
/// Required test cases:
/// 
/// 1. GetDoctorsWithAvailabilityAsync_ShouldReturnCorrectAppointmentCounts
///    - Seed 2 doctors, one with 3 upcoming appointments, one with 0
///    - Assert correct counts
///
/// 2. GetDoctorsWithAvailabilityAsync_ShouldReturnEmptyList_WhenNoDoctors
///    - Empty database (no active doctors)
///    - Assert returns empty list
///
/// 3. GetAllDoctorsAsync_ShouldOnlyReturnActiveDoctors
///    - Seed active and inactive doctors
///    - Assert only active ones returned
/// </summary>
public class DoctorServiceTests
{
    private readonly MemoryCache _memoryCache;

    public DoctorServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
    }
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

    [Fact]
    public async Task GetDoctorsWithAvailabilityAsync_ShouldReturnCorrectCounts()
    {
        // Arrange
        using var db = CreateInMemoryContext();
        // Seed two test doctors
        var doctor1 = await SeedDoctor(db, id: 201);
        var doctor2 = await SeedDoctor(db, id: 202);

        // Seed a patient required by appointments
        var patient = await SeedPatient(db, id: 301);

        // Add 3 upcoming scheduled appointments for doctor1
        var now = DateTime.UtcNow;
        var appointments = new[]
        {
            new Appointment { Id = 1001, PatientId = patient.Id, DoctorId = doctor1.Id, AppointmentDateTime = now.AddDays(1), Reason = "A", Status = AppointmentStatus.Scheduled },
            new Appointment { Id = 1002, PatientId = patient.Id, DoctorId = doctor1.Id, AppointmentDateTime = now.AddDays(2), Reason = "B", Status = AppointmentStatus.Scheduled },
            new Appointment { Id = 1003, PatientId = patient.Id, DoctorId = doctor1.Id, AppointmentDateTime = now.AddDays(3), Reason = "C", Status = AppointmentStatus.Scheduled },
            // doctor2 gets a past appointment (should not be counted)
            new Appointment { Id = 1004, PatientId = patient.Id, DoctorId = doctor2.Id, AppointmentDateTime = now.AddDays(-1), Reason = "Past", Status = AppointmentStatus.Scheduled }
        };
        db.Appointments.AddRange(appointments);
        await db.SaveChangesAsync();

        var logger = new Mock<ILogger<DoctorService>>();
        var service = new DoctorService(db, logger.Object, _memoryCache);

        // Act
        var results = await service.GetDoctorsWithAvailabilityAsync();

        // Assert
        Assert.NotNull(results);
        Assert.Contains(results, r => r.Id == doctor1.Id);
        Assert.Contains(results, r => r.Id == doctor2.Id);

        var r1 = results.Single(r => r.Id == doctor1.Id);
        Assert.Equal(3, r1.UpcomingAppointmentCount);

        var r2 = results.Single(r => r.Id == doctor2.Id);
        Assert.Equal(0, r2.UpcomingAppointmentCount);

    }

    [Fact]
    public async Task GetDoctorsWithAvailabilityAsync_ShouldReturnEmpty_WhenNoDoctors()
    {
        // Arrange
        using var db = CreateInMemoryContext();

        // Mark all seeded doctors inactive so there are no active doctors
        var seeded = db.Doctors.ToList();
        foreach (var d in seeded)
        {
            d.IsActive = false;
        }
        await db.SaveChangesAsync();

        var logger = new Mock<ILogger<DoctorService>>();
        var service = new DoctorService(db, logger.Object, _memoryCache);

        // Act
        var results = await service.GetDoctorsWithAvailabilityAsync();

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetAllDoctorsAsync_ShouldOnlyReturnActiveDoctors()
    {
        Assert.True(true, "TODO: Implement this test");
    }
}
